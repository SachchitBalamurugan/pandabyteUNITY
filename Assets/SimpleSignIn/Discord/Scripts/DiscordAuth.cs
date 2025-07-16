using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.SimpleSignIn.Discord.Scripts
{
    /// <summary>
    /// API specification: https://discord.com/developers/docs/topics/oauth2
    /// </summary>
    public partial class DiscordAuth
    {
        public SavedAuth SavedAuth { get; private set; }
        public TokenResponse TokenResponse { get; private set; }
        public bool DebugLog = true;

        private const string AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
        private const string TokenEndpoint = "https://discord.com/api/oauth2/token";
        private const string UserInfoEndpoint = "https://discord.com/api/users/@me";
        private const string RevocationEndpoint = "https://discord.com/api/oauth2/token/revoke";

        private readonly DiscordAuthSettings _settings;
        private Implementation _implementation;
        private string _redirectUri, _state, _verifier;
        private Action<bool, string, UserInfo> _callbackU;
        private Action<bool, string, TokenResponse> _callbackT;

        /// <summary>
        /// A constructor that accepts an instance of DiscordAuthSettings. If Null is passed, it will load default settings from Resources (DiscordAuthSettings scriptable object).
        /// </summary>
        public DiscordAuth(DiscordAuthSettings settings = null)
        {
            _settings = settings == null ? Resources.Load<DiscordAuthSettings>("DiscordAuthSettings") : settings;

            if (_settings == null) throw new NullReferenceException(nameof(_settings));

            SavedAuth = SavedAuth.GetInstance(_settings.ClientId);
            Application.deepLinkActivated += OnDeepLinkActivated;

            #if UNITY_IOS && !UNITY_EDITOR

            SafariViewController.DidCompleteInitialLoad += DidCompleteInitialLoad;
            SafariViewController.DidFinish += UserCancelledHook;

            #endif
        }

        /// <summary>
        /// A destructor.
        /// </summary>
        ~DiscordAuth()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;

            #if UNITY_IOS && !UNITY_EDITOR

            SafariViewController.DidCompleteInitialLoad -= DidCompleteInitialLoad;
            SafariViewController.DidFinish -= UserCancelledHook;

            #endif
        }

        /// <summary>
        /// Performs sign-in and returns an instance of UserInfo with `callback`. If `caching` is True, it will return the previously saved UserInfo.
        /// </summary>
        public void SignIn(Action<bool, string, UserInfo> callback, bool caching = true)
        {
            _callbackU = callback;
            _callbackT = null;

            Initialize();

            if (SavedAuth == null)
            {
                Auth();
            }
            else if (caching && SavedAuth.UserInfo != null)
            {
                callback(true, null, SavedAuth.UserInfo);
            }
            else
            {
                UseSavedToken();
            }
        }

        /// <summary>
        /// Returns an access token. User data can be obtained from TokenResponse.IdToken (JWT).
        /// </summary>
        public void GetTokenResponse(Action<bool, string, TokenResponse> callback)
        {
            _callbackU = null;
            _callbackT = callback;

            Initialize();

            if (SavedAuth == null)
            {
                Auth();
            }
            else
            {
                if (SavedAuth.TokenResponse.Expired)
                {
                    Log("Refreshing expired access token...");
                    RefreshAccessToken(callback);
                }
                else
                {
                    callback(true, null, SavedAuth.TokenResponse);
                }
            }
        }

        /// <summary>
        /// Performs sign-out.
        /// </summary>
        public void SignOut(bool revokeAccessToken = false)
        {
            TokenResponse = null;

            if (SavedAuth != null)
            {
                if (revokeAccessToken && SavedAuth.TokenResponse != null)
                {
                    RevokeAccessToken(SavedAuth.TokenResponse.AccessToken);
                }

                SavedAuth.Delete();
                SavedAuth = null;
            }
        }

        /// <summary>
        /// Force cancel.
        /// </summary>
        public void Cancel()
        {
            _redirectUri = _state = _verifier = null;
            _callbackU = null;
            _callbackT = null;
            ApplicationFocusHook.Cancel();
        }

        private const string TempKey = "oauth.temp";

        /// <summary>
        /// This can be called on app startup to continue oauth.
        /// In some scenarios, the app may be terminated while the user performs sign-in on Discord website.
        /// </summary>
        public void TryResume(Action<bool, string, UserInfo> callbackUserInfo = null, Action<bool, string, TokenResponse> callbackTokenResponse = null)
        {
            if (string.IsNullOrEmpty(Application.absoluteURL) || !PlayerPrefs.HasKey(TempKey)) return;

            var parts = PlayerPrefs.GetString(TempKey).Split('|');

            if (!Application.absoluteURL.StartsWith(parts[2])) return;

            _state = parts[0];
            _verifier = parts[1];
            _redirectUri = parts[2];
            _callbackU = callbackUserInfo;
            _callbackT = callbackTokenResponse;

            OnDeepLinkActivated(Application.absoluteURL);
        }

        private void Initialize()
        {
            #if UNITY_EDITOR

            _implementation = Implementation.LoopbackFlow;
            _redirectUri = "http://localhost:20561/";

            #elif UNITY_WEBGL

            _implementation = Implementation.AuthorizationMiddleware;
            _redirectUri = "";
            
            #elif UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_WSA || UNITY_STANDALONE_WIN

            _implementation = Implementation.AuthorizationMiddleware;
            _redirectUri = $"{_settings.CustomUriScheme}:/oauth2/discord";

            #if UNITY_STANDALONE_WIN

            WindowsDeepLinking.Initialize(_settings.CustomUriScheme, OnDeepLinkActivated);

            #endif

            #endif

            if (SavedAuth != null && SavedAuth.ClientId != _settings.ClientId)
            {
                SavedAuth.Delete();
                SavedAuth = null;
            }
        }

        private void Auth()
        {
            _state = Guid.NewGuid().ToString("N");
            _verifier = Guid.NewGuid().ToString("N");

            PlayerPrefs.SetString("oauth.temp", $"{_state}|{_verifier}|{_redirectUri}");
            PlayerPrefs.Save();

            if (!_settings.ManualCancellation)
            {
                #if UNITY_IOS && !UNITY_EDITOR

                if (!_settings.UseSafariViewController) ApplicationFocusHook.Create(UserCancelledHook);

                #else

                ApplicationFocusHook.Create(UserCancelledHook);

                #endif
            }

            var authorizationRequest = $"{AuthorizationEndpoint}?response_type=code&client_id={_settings.ClientId}&scope={Uri.EscapeDataString(string.Join(" ", _settings.AccessScopes))}&state={_state}&prompt={_settings.Prompt}&integration_type={_settings.IntegrationType}";

            if (_implementation == Implementation.AuthorizationMiddleware)
            {
                var redirectUri = AuthorizationMiddleware.Endpoint + "/redirect";

                authorizationRequest += $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";
                
                AuthorizationMiddleware.Auth(_redirectUri, _state, () => AuthorizationRequest(authorizationRequest), (success, error, code) =>
                {
                    if (success)
                    {
                        PerformCodeExchange(code);
                    }
                    else
                    {
                        _callbackU?.Invoke(false, error, null);
                        _callbackT?.Invoke(false, error, null);
                    }
                });
            }
            else
            {
                authorizationRequest += $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}";

                AuthorizationRequest(authorizationRequest);

                switch (_implementation)
                {
                    case Implementation.LoopbackFlow:
                        LoopbackFlow.Initialize(_redirectUri, OnDeepLinkActivated);
                        break;
                }
            }
        }

        private void AuthorizationRequest(string url)
        {
            Log($"Authorization: {url}");

            #if UNITY_IOS && !UNITY_EDITOR

            if (_settings.UseSafariViewController)
            {
                SafariViewController.OpenURL(url);
            }
            else
            {
                Application.OpenURL(url);
            }

            #else

            Application.OpenURL(url);

            #endif
        }

        private void DidCompleteInitialLoad(bool loaded)
        {
            if (loaded) return;

            const string error = "Failed to load auth screen.";

            _callbackT?.Invoke(false, error, null);
            _callbackU?.Invoke(false, error, null);
        }

        private async void UserCancelledHook()
        {
            if (_settings.ManualCancellation) return;

            var time = DateTime.UtcNow;

            while ((DateTime.UtcNow - time).TotalSeconds < 1)
            {
                await Task.Yield();
            }

            if (_verifier == null) return;

            _verifier = null;

            const string error = "User cancelled.";

            _callbackT?.Invoke(false, error, null);
            _callbackU?.Invoke(false, error, null);
        }

        private void UseSavedToken()
        {
            if (SavedAuth == null || SavedAuth.ClientId != _settings.ClientId)
            {
                SignOut();
                SignIn(_callbackU);
            }
            else if (!SavedAuth.TokenResponse.Expired)
            {
                Log("Using saved access token...");
                RequestUserInfo(SavedAuth.TokenResponse.AccessToken, (success, _, userInfo) =>
                {
                    if (success)
                    {
                        _callbackU(true, null, userInfo);
                    }
                    else
                    {
                        SignOut();
                        SignIn(_callbackU);
                    }
                });
            }
            else
            {
                Log("Refreshing expired access token...");
                RefreshAccessToken((success, _, _) =>
                {
                    if (success)
                    {
                        RequestUserInfo(SavedAuth.TokenResponse.AccessToken, _callbackU);
                    }
                    else
                    {
                        SignOut();
                        SignIn(_callbackU);
                    }
                });
            }
        }

        private void OnDeepLinkActivated(string deepLink)
        {
            Log($"Deep link activated: {deepLink}");

            deepLink = deepLink.Replace(":///", ":/"); // Some browsers may add extra slashes.

            if (_redirectUri == null || !deepLink.StartsWith(_redirectUri) || _verifier == null)
            {
                Log("Unexpected deep link.");
                return;
            }

            #if UNITY_IOS && !UNITY_EDITOR

            if (_settings.UseSafariViewController)
            {
                Log($"Closing SafariViewController");
                SafariViewController.Close();
            }
            
            #endif

            var parameters = Helpers.ParseQueryString(deepLink);
            var error = parameters.Get("error");

            if (error != null)
            {
                _callbackU?.Invoke(false, error, null);
                _callbackT?.Invoke(false, error, null);
                return;
            }

            var state = parameters.Get("state");
            var code = parameters.Get("code");

            if (state == null || code == null) return;

            if (state == _state)
            {
                PerformCodeExchange(code);
            }
            else
            {
                Log("Unexpected response.");
            }
        }

        private void PerformCodeExchange(string code)
        {
            var redirectUri = AuthorizationMiddleware.Endpoint + "/redirect";
            var formFields = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri }
            };

            _verifier = null;

            var request = UnityWebRequest.Post(TokenEndpoint, formFields);
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

            request.SetRequestHeader("Authorization", $"Basic {auth}");

            Log($"Exchanging code for access token: {request.url}");

            request.SendWebRequest().completed += _ =>
            {
                if (request.error == null)
                {
                    Log($"TokenExchangeResponse={request.downloadHandler.text}");

                    TokenResponse = TokenResponse.Parse(request.downloadHandler.text);
                    SavedAuth = new SavedAuth(_settings.ClientId, TokenResponse);
                    SavedAuth.Save();

                    if (_callbackT != null)
                    {
                        _callbackT(true, null, TokenResponse);
                    }

                    if (_callbackU != null)
                    {
                        RequestUserInfo(TokenResponse.AccessToken, _callbackU);
                    }
                }
                else
                {
                    _callbackU?.Invoke(false, request.GetError(), null);
                    _callbackT?.Invoke(false, request.GetError(), null);
                }
            };

            if (PlayerPrefs.HasKey(TempKey))
            {
                PlayerPrefs.DeleteKey(TempKey);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// You can move this function to your backend for more security.
        /// </summary>
        public void RequestUserInfo(string accessToken, Action<bool, string, UserInfo> callback)
        {
            var request = UnityWebRequest.Get(UserInfoEndpoint);

            Log($"Requesting user info: {request.url}");

            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            request.SendWebRequest().completed += _ =>
            {
                if (request.error == null)
                {
                    Log($"UserInfo={request.downloadHandler.text}");
                    SavedAuth.UserInfo = JsonConvert.DeserializeObject<UserInfo>(request.downloadHandler.text);
                    SavedAuth.Save();
                    callback(true, null, SavedAuth.UserInfo);
                }
                else
                {
                    callback(false, request.GetError(), null);
                }
            };
        }

        /// <summary>
        /// https://discord.com/developers/docs/topics/oauth2#authorization-code-grant-refresh-token-exchange-example
        /// </summary>
        public void RefreshAccessToken(Action<bool, string, TokenResponse> callback)
        {
            if (SavedAuth == null) throw new Exception("Initial authorization is required.");

            var refreshToken = SavedAuth.TokenResponse.RefreshToken;
            var formFields = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            var request = UnityWebRequest.Post(TokenEndpoint, formFields);
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

            request.SetRequestHeader("Authorization", $"Basic {auth}");

            Log($"Access token refresh: {request.url}");

            request.SendWebRequest().completed += _ =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Log($"TokenExchangeResponse={request.downloadHandler.text}");

                    TokenResponse = TokenResponse.Parse(request.downloadHandler.text);
                    SavedAuth.TokenResponse = TokenResponse;
                    SavedAuth.Save();
                    callback(true, null, TokenResponse);
                }
                else
                {
                    Debug.LogError(request.GetError());
                    callback(false, request.GetError(), null);
                }

                request.Dispose();
            };
        }

        private void RevokeAccessToken(string accessToken)
        {
            var formFields = new Dictionary<string, string>
            {
                { "token", accessToken },
                { "token_type_hint", "access_token" }
            };
            var request = UnityWebRequest.Post(RevocationEndpoint, formFields);
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

            request.SetRequestHeader("Authorization", $"Basic {auth}");

            Log($"Revoking access token: {request.url}");

            request.SendWebRequest().completed += _ => Log(request.error ?? "Access token revoked!");
        }

        private void Log(string message)
        {
            if (DebugLog)
            {
                Debug.Log(message); // TODO: Remove in Release.
            }
        }
    }
}