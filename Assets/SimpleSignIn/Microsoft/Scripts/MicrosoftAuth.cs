using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.SimpleSignIn.Microsoft.Scripts
{
    /// <summary>
    /// API specification: https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow
    /// </summary>
    public partial class MicrosoftAuth
    {
        public SavedAuth SavedAuth { get; private set; }
        public TokenResponse TokenResponse { get; private set; }
        public string ClientId => _settings.ClientId;
        public bool DebugLog = true;

        /// <summary>
        /// OpenID configuration: https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration
        /// </summary>
        private const string AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
        private const string TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
        private const string UserInfoEndpoint = "https://graph.microsoft.com/oidc/userinfo";
        private const string LogoutEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/logout";

        private readonly MicrosoftAuthSettings _settings;
        private Implementation _implementation;
        private string _redirectUri, _state, _codeVerifier, _nonce;
        private Action<bool, string, UserInfo> _callbackU;
        private Action<bool, string, TokenResponse> _callbackT;

        /// <summary>
        /// A constructor that accepts an instance of MicrosoftAuthSettings. If Null is passed, it will load default settings from Resources (MicrosoftAuthSettings scriptable object).
        /// </summary>
        public MicrosoftAuth(MicrosoftAuthSettings settings = null)
        {
            _settings = settings == null ? Resources.Load<MicrosoftAuthSettings>("MicrosoftAuthSettings") : settings;

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
        ~MicrosoftAuth()
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
        public void SignOut(bool logout = false)
        {
            TokenResponse = null;

            if (SavedAuth != null)
            {
                if (logout && SavedAuth.TokenResponse != null)
                {
                    Logout();
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
            _redirectUri = _state = _codeVerifier = null;
            _callbackU = null;
            _callbackT = null;
            ApplicationFocusHook.Cancel();
        }

        private const string TempKey = "oauth.temp";

        /// <summary>
        /// This can be called on app startup to continue oauth.
        /// In some scenarios, the app may be terminated while the user performs sign-in.
        /// </summary>
        public void TryResume(Action<bool, string, UserInfo> callbackUserInfo = null, Action<bool, string, TokenResponse> callbackTokenResponse = null)
        {
            if (string.IsNullOrEmpty(Application.absoluteURL) || !PlayerPrefs.HasKey(TempKey)) return;

            var parts = PlayerPrefs.GetString(TempKey).Split('|');

            if (!Application.absoluteURL.StartsWith(parts[2])) return;

            _state = parts[0];
            _codeVerifier = parts[1];
            _redirectUri = parts[2];
            _callbackU = callbackUserInfo;
            _callbackT = callbackTokenResponse;

            OnDeepLinkActivated(Application.absoluteURL);
        }

        private void Initialize()
        {
            #if UNITY_EDITOR

            _implementation = Implementation.LoopbackFlow;
            _redirectUri = $"http://localhost:{Helpers.GetRandomUnusedPort()}/";
            
            #elif UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_WSA

            _implementation = Implementation.DeepLinking;
            _redirectUri = $"{_settings.CustomUriScheme}://oauth2/ms";

            #elif UNITY_STANDALONE_WIN

            _implementation = _settings.CustomUriScheme == "" ? Implementation.LoopbackFlow : Implementation.DeepLinking;
            _redirectUri = _settings.CustomUriScheme == "" ? $"http://localhost:{Helpers.GetRandomUnusedPort()}/" : $"{_settings.CustomUriScheme}://oauth2/ms";

            if (_implementation == Implementation.DeepLinking)
			{
				WindowsDeepLinking.Initialize(_settings.CustomUriScheme, OnDeepLinkActivated);
			}

            #elif UNITY_WEBGL

            _implementation = Implementation.AuthorizationMiddleware;
            _redirectUri = "";

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
            _codeVerifier = Guid.NewGuid().ToString("N");
            _nonce = Guid.NewGuid().ToString("N");

            PlayerPrefs.SetString("oauth.temp", $"{_state}|{_codeVerifier}|{_redirectUri}");
            PlayerPrefs.Save();

            if (!_settings.ManualCancellation)
            {
                #if UNITY_IOS && !UNITY_EDITOR

                if (!_settings.UseSafariViewController) ApplicationFocusHook.Create(UserCancelledHook);

                #else

                ApplicationFocusHook.Create(UserCancelledHook);

                #endif
            }

            var codeChallenge = Helpers.CreateCodeChallenge(_codeVerifier);
            var redirectUri = _implementation == Implementation.AuthorizationMiddleware ? AuthorizationMiddleware.Endpoint + "/redirect" : _redirectUri;
            var authorizationRequest = $"{AuthorizationEndpoint}?client_id={_settings.ClientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_mode=query&scope={Uri.EscapeDataString(string.Join(" ", _settings.AccessScopes))}&state={_state}&code_challenge={codeChallenge}&code_challenge_method=S256&nonce={_nonce}";

            if (_implementation == Implementation.AuthorizationMiddleware)
            {
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

            if (_codeVerifier == null) return;

            _codeVerifier = null;

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

            if (_redirectUri == null || !deepLink.StartsWith(_redirectUri) || _codeVerifier == null)
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
            var redirectUri = _implementation == Implementation.AuthorizationMiddleware ? AuthorizationMiddleware.Endpoint + "/redirect" : _redirectUri;
            var formFields = new Dictionary<string, string>
            {
                { "client_id", _settings.ClientId },
                { "scope", string.Join(" ", _settings.AccessScopes) },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" },
                { "code_verifier", _codeVerifier }
            };

            if (_implementation == Implementation.LoopbackFlow || _implementation == Implementation.AuthorizationMiddleware)
            {
                //formFields.Add("client_secret", "");
            }

            _codeVerifier = null;

            var request = UnityWebRequest.Post(TokenEndpoint, formFields);

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

                request.Dispose();
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

                request.Dispose();
            };
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow#refresh-the-access-token
        /// </summary>
        public void RefreshAccessToken(Action<bool, string, TokenResponse> callback)
        {
            if (SavedAuth == null) throw new Exception("Initial authorization is required.");

            var refreshToken = SavedAuth.TokenResponse.RefreshToken;
            var formFields = new Dictionary<string, string>
            {
                { "client_id", _settings.ClientId },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            //if (_implementation == Implementation.LoopbackFlow || _implementation == Implementation.AuthorizationMiddleware)
            //{
            //    formFields.Add("client_secret", _settings.ClientSecret);
            //}

            var request = UnityWebRequest.Post(TokenEndpoint, formFields);

            Log($"Access token refresh: {request.url}");

            request.SendWebRequest().completed += _ =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Log($"TokenExchangeResponse={request.downloadHandler.text}");

                    TokenResponse = TokenResponse.Parse(request.downloadHandler.text);
                    TokenResponse.RefreshToken = refreshToken;
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

        private void Logout()
        {
            AuthorizationRequest(LogoutEndpoint);
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