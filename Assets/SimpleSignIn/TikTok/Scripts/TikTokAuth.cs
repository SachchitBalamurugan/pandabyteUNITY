using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.SimpleSignIn.TikTok.Scripts
{
    /// <summary>
    /// API specification:
    /// https://developers.tiktok.com/doc/login-kit-overview/
    /// https://developers.tiktok.com/doc/login-kit-manage-user-access-tokens/
    /// https://developers.tiktok.com/doc/tiktok-api-v2-get-user-info
    /// </summary>
    public partial class TikTokAuth
    {
        public SavedAuth SavedAuth { get; private set; }
        public TokenResponse TokenResponse { get; private set; }
        public bool DebugLog = true;

        /// <summary>
        /// OpenID configuration:
        /// </summary>
        private const string AuthorizationEndpoint = "https://www.tiktok.com/v2/auth/authorize/";
        private const string TokenEndpoint = "https://open.tiktokapis.com/v2/oauth/token/";
        private const string UserInfoEndpoint = "https://open.tiktokapis.com/v2/user/info/";
        private const string RevocationEndpoint = "https://open.tiktokapis.com/v2/oauth/revoke/";

        private readonly TikTokAuthSettings _settings;
        private Implementation _implementation;
        private string _redirectUri, _state, _codeVerifier;
        private Action<bool, string, UserInfo> _callbackU;
        private Action<bool, string, TokenResponse> _callbackT;

        /// <summary>
        /// A constructor that accepts an instance of TikTokAuthSettings. If Null is passed, it will load default settings from Resources (TikTokAuthSettings scriptable object).
        /// </summary>
        public TikTokAuth(TikTokAuthSettings settings = null)
        {
            _settings = settings == null ? Resources.Load<TikTokAuthSettings>("TikTokAuthSettings") : settings;

            if (_settings == null) throw new NullReferenceException(nameof(_settings));

            SavedAuth = SavedAuth.GetInstance(_settings.ClientKey);
            Application.deepLinkActivated += OnDeepLinkActivated;

            #if UNITY_IOS && !UNITY_EDITOR

            SafariViewController.DidCompleteInitialLoad += DidCompleteInitialLoad;
            SafariViewController.DidFinish += UserCancelledHook;

            #endif
        }

        /// <summary>
        /// A destructor.
        /// </summary>
        ~TikTokAuth()
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

            UseLoopback();

            #elif UNITY_ANDROID

            switch (_settings.FlowForAndroid)
            {
                case OAuthFlow.AuthorizationMiddleware: UseMiddleware(); break;
                case OAuthFlow.Android: UseDeepLinking(); break;
                default: throw new NotSupportedException();
            }

            #elif UNITY_IOS

            switch (_settings.FlowForIOS)
            {
                case OAuthFlow.AuthorizationMiddleware: UseMiddleware(); break;
                case OAuthFlow.iOS: UseDeepLinking(); break;
                default: throw new NotSupportedException();
            }

            #elif UNITY_STANDALONE_WIN

            switch (_settings.FlowForWindows)
            {
                case OAuthFlow.Loopback: UseLoopback(); break;
                case OAuthFlow.AuthorizationMiddleware: UseMiddleware(); break;
                case OAuthFlow.Android:
                case OAuthFlow.iOS: UseDeepLinking(); break;
            }

            #elif UNITY_WSA

            switch (_settings.FlowForUWP)
            {
                case OAuthFlow.Loopback: UseLoopback(); break;
                case OAuthFlow.AuthorizationMiddleware: UseMiddleware(); break;
                case OAuthFlow.Android:
                case OAuthFlow.iOS: UseDeepLinking(); break;
            }

            #elif UNITY_STANDALONE_OSX

            switch (_settings.FlowForOSX)
            {
                case OAuthFlow.Loopback: UseLoopback(); break;
                case OAuthFlow.AuthorizationMiddleware: UseMiddleware(); break;
                case OAuthFlow.Android:
                case OAuthFlow.iOS: UseDeepLinking(); break;
            }

            #elif UNITY_WEBGL

            UseMiddleware();

            #endif

            if (SavedAuth != null && SavedAuth.ClientId != _settings.ClientKey)
            {
                SavedAuth.Delete();
                SavedAuth = null;
            }

            void UseLoopback()
            {
                _implementation = Implementation.LoopbackFlow;
                _redirectUri = $"http://localhost:{Helpers.GetRandomUnusedPort()}/";
            }

            void UseDeepLinking()
            {
                _implementation = Implementation.DeepLinking;
                _redirectUri = $"{_settings.CustomUriScheme}://oauth2/tiktok";
            }

            void UseMiddleware()
            {
                _implementation = Implementation.AuthorizationMiddleware;

                #if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_STANDALONE_OSX

                _redirectUri = $"{_settings.CustomUriScheme}:/oauth2/tiktok";

                #else

                _redirectUri = "";

                #endif

                #if UNITY_STANDALONE_WIN

                WindowsDeepLinking.Initialize(_settings.CustomUriScheme, OnDeepLinkActivated);

                #endif
            }
        }

        private void Auth()
        {
            _state = Guid.NewGuid().ToString("N");
            _codeVerifier = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

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

            var codeChallenge = Helpers.ComputeHashSha256(_codeVerifier);
            var redirectUri = _implementation == Implementation.AuthorizationMiddleware ? AuthorizationMiddleware.Endpoint + "/redirect" : _redirectUri;
            var authorizationRequest = $"{AuthorizationEndpoint}?client_key={_settings.ClientKey}&scope={Uri.EscapeDataString(string.Join(",", _settings.AccessScopes))}&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={_state}&response_type=code&code_challenge={Uri.EscapeDataString(codeChallenge)}&code_challenge_method=S256";
            
            #if !UNITY_EDITOR

            #if UNITY_ANDROID

            if (_implementation == Implementation.DeepLinking) SetupQueryForAndroid(Application.identifier);

            #elif UNITY_IOS

            if (_implementation == Implementation.DeepLinking) SetupQueryForiOS(Application.identifier);

            #elif UNITY_STANDALONE_WIN

            if (_settings.FlowForWindows == OAuthFlow.Android) SetupQueryForAndroid(_settings.PackageName); else if (_settings.FlowForWindows == OAuthFlow.iOS) SetupQueryForiOS(_settings.PackageName);

            #elif UNITY_WSA

            if (_settings.FlowForUWP == OAuthFlow.Android) SetupQueryForAndroid(_settings.PackageName); else if (_settings.FlowForUWP == OAuthFlow.iOS) SetupQueryForiOS(_settings.PackageName);

            #elif UNITY_STANDALONE_OSX

            if (_settings.FlowForOSX == OAuthFlow.Android) SetupQueryForAndroid(_settings.PackageName); else if (_settings.FlowForOSX == OAuthFlow.iOS) SetupQueryForiOS(_settings.PackageName);

            #endif

            void SetupQueryForAndroid(string packageName)
            {
                authorizationRequest += $"&sdk_name=tiktok_sdk_auth&device_platform=android&app_identity={Helpers.ComputeHashSha256(packageName)}&certificate={_settings.AndroidSha256Fingerprint}";
            }

            void SetupQueryForiOS(string packageName)
            {
                authorizationRequest += $"&device_platform=iphone&app_identity={Helpers.ComputeHashSha512(packageName)}";
            }

            #endif

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
            if (SavedAuth == null || SavedAuth.ClientId != _settings.ClientKey)
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
                { "client_key", _settings.ClientKey },
                { "client_secret", _settings.ClientSecret },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", redirectUri },
                { "code_verifier", _codeVerifier }
            };

            _codeVerifier = null;

            #if UNITY_WEBGL // CORS workaround.

            var request = UnityWebRequest.Post($"{AuthorizationMiddleware.Endpoint}/download", new Dictionary<string, string> { { "url", TokenEndpoint }, { "form", JsonConvert.SerializeObject(formFields) } });
            
            #else

            var request = UnityWebRequest.Post(TokenEndpoint, formFields);

            #endif

            Log($"Exchanging code for access token: {request.url}");

            request.SendWebRequest().completed += _ =>
            {
                var error = ReadError(request);

                if (error == null)
                {
                    Log($"TokenExchangeResponse={request.downloadHandler.text}");

                    TokenResponse = TokenResponse.Parse(request.downloadHandler.text);
                    SavedAuth = new SavedAuth(_settings.ClientKey, TokenResponse);
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
                    _callbackU?.Invoke(false, error, null);
                    _callbackT?.Invoke(false, error, null);
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
            var request = UnityWebRequest.Get($"{UserInfoEndpoint}?fields={string.Join(',', _settings.UserFields)}");

            Log($"Requesting user info: {request.url}");

            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            request.SendWebRequest().completed += _ =>
            {
                if (request.error == null)
                {
                    var json = request.downloadHandler.text;

                    request.Dispose();

                    Log($"UserInfo={json}");

                    var jObject = JObject.Parse(json);
                    var error = jObject["error"];

                    if (error != null)
                    {
                        var errorMessage = (string) error["Message"];

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            callback(false, errorMessage, null);
                            return;
                        }
                    }

                    var data = jObject["data"];

                    if (data == null)
                    {
                        callback(false, "[data] expected.", null);
                        return;
                    }

                    var user = data["user"];

                    if (user == null)
                    {
                        callback(false, "[user] expected.", null);
                        return;
                    }

                    SavedAuth.UserInfo = user.ToObject<UserInfo>();
                    SavedAuth.Save();
                    callback(true, null, SavedAuth.UserInfo);
                }
                else
                {
                    callback(false, request.GetError(), null);
                    request.Dispose();
                }
            };
        }

        public void RefreshAccessToken(Action<bool, string, TokenResponse> callback)
        {
            if (SavedAuth == null) throw new Exception("Initial authorization is required.");

            var refreshToken = SavedAuth.TokenResponse.RefreshToken;
            var formFields = new Dictionary<string, string>
            {
                { "client_key", _settings.ClientKey },
                { "client_secret", _settings.ClientSecret },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };
            var request = CreateWebRequest(TokenEndpoint, formFields);

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

        private void RevokeAccessToken(string accessToken)
        {
            var formFields = new Dictionary<string, string>
            {
                { "client_key", _settings.ClientKey },
                { "client_secret", _settings.ClientSecret },
                { "token", accessToken }
            };
            var request = CreateWebRequest(RevocationEndpoint, formFields);
            
            Log($"Revoking access token: {request.url}");

            request.SendWebRequest().completed += _ =>
            {
                var error = ReadError(request);

                Log(error ?? "Access token revoked!");

                request.Dispose();
            };
        }

        private static UnityWebRequest CreateWebRequest(string url, Dictionary<string, string> formFields = null)
        {
            #if UNITY_WEBGL // CORS workaround.

            var dict = new Dictionary<string, string> { { "url", url } };

            if (formFields != null)
            {
                dict.Add("form", JsonConvert.SerializeObject(formFields));
            }

            return UnityWebRequest.Post($"{AuthorizationMiddleware.Endpoint}/download", dict);
            
            #else

            return formFields == null ? UnityWebRequest.Get(url) : UnityWebRequest.Post(url, formFields);

            #endif
        }

        private static string ReadError(UnityWebRequest request)
        {
            if (request.error != null) return request.GetError();

            var jObject = JObject.Parse(request.downloadHandler.text);

            if (jObject.TryGetValue("error_description", out var description)) return (string) description;
            if (jObject.TryGetValue("error", out var error)) return (string) error;

            return request.GetError();
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