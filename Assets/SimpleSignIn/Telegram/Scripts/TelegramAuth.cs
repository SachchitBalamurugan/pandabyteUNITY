using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.SimpleSignIn.Telegram.Scripts
{
    /// <summary>
    /// API specification: https://core.telegram.org/bots
    /// </summary>
    public partial class TelegramAuth
    {
        public SavedAuth SavedAuth { get; private set; }
        public bool DebugLog = true;

        private readonly TelegramAuthSettings _settings;
        private Implementation _implementation;
        private string _redirectUri, _state;
        private Action<bool, string, UserInfo> _callback;

        /// <summary>
        /// A constructor that accepts an instance of TelegramAuthSettings. If Null is passed, it will load default settings from Resources (TelegramAuthSettings scriptable object).
        /// </summary>
        public TelegramAuth(TelegramAuthSettings settings = null)
        {
            _settings = settings == null ? Resources.Load<TelegramAuthSettings>("TelegramAuthSettings") : settings;

            if (_settings == null) throw new NullReferenceException(nameof(_settings));

            SavedAuth = SavedAuth.GetInstance(_settings.BotName);
            Application.deepLinkActivated += OnDeepLinkActivated;

            #if UNITY_IOS && !UNITY_EDITOR

            SafariViewController.DidCompleteInitialLoad += DidCompleteInitialLoad;
            SafariViewController.DidFinish += UserCancelledHook;

            #endif
        }

        /// <summary>
        /// A destructor.
        /// </summary>
        ~TelegramAuth()
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
            _callback = callback;

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
                Auth();
            }
        }

        /// <summary>
        /// Performs sign-out.
        /// </summary>
        public void SignOut()
        {
            if (SavedAuth != null)
            {
                SavedAuth.Delete();
                SavedAuth = null;
            }
        }

        /// <summary>
        /// Force cancel.
        /// </summary>
        public void Cancel()
        {
            _redirectUri = _state;
            _callback = null;
            ApplicationFocusHook.Cancel();
        }

        private const string TempKey = "oauth.temp";

        /// <summary>
        /// This can be called on app startup to continue oauth.
        /// In some scenarios, the app may be terminated while the user performs sign-in.
        /// </summary>
        public void TryResume(Action<bool, string, UserInfo> callbackUserInfo = null)
        {
            if (string.IsNullOrEmpty(Application.absoluteURL) || !PlayerPrefs.HasKey(TempKey)) return;

            var parts = PlayerPrefs.GetString(TempKey).Split('|');

            if (!Application.absoluteURL.StartsWith(parts[2])) return;

            _state = parts[0];
            _redirectUri = parts[1];
            _callback = callbackUserInfo;

            OnDeepLinkActivated(Application.absoluteURL);
        }

        private void Initialize()
        {
            #if UNITY_EDITOR || UNITY_WEBGL

            _implementation = Implementation.AuthorizationMiddleware;
            _redirectUri = "";
            
            #elif UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_WSA

            _implementation = Implementation.AuthorizationMiddleware;
            _redirectUri = $"{_settings.CustomUriScheme}:/oauth2/telegram";

            #elif UNITY_STANDALONE_WIN

            _implementation = Implementation.AuthorizationMiddleware;
            _redirectUri = $"{_settings.CustomUriScheme}:/oauth2/telegram";

            WindowsDeepLinking.Initialize(_settings.CustomUriScheme, OnDeepLinkActivated);

            #endif

            if (SavedAuth != null && SavedAuth.BotId != _settings.BotName)
            {
                SavedAuth.Delete();
                SavedAuth = null;
            }
        }

        private void Auth()
        {
            _state = Guid.NewGuid().ToString("N");

            PlayerPrefs.SetString(TempKey, $"{_state}|{_redirectUri}");
            PlayerPrefs.Save();

            if (!_settings.ManualCancellation)
            {
                #if UNITY_IOS && !UNITY_EDITOR

                if (!_settings.UseSafariViewController) ApplicationFocusHook.Create(UserCancelledHook);

                #else

                ApplicationFocusHook.Create(UserCancelledHook);

                #endif
            }

            var authorizationRequest = _settings.ShowTelegramWidget
                ? AuthorizationMiddleware.Endpoint + $"/telegram_auth?id={_settings.BotName}&state={_state}&widget=1"
                : AuthorizationMiddleware.Endpoint + $"/telegram_auth?id={_settings.BotId}&state={_state}&widget=0";

            if (_implementation == Implementation.AuthorizationMiddleware)
            {
                AuthorizationMiddleware.Auth(_redirectUri, _state, () => AuthorizationRequest(authorizationRequest), (success, error, code) =>
                {
                    if (success)
                    {
                        DecodeAuthResult(code);
                    }
                    else
                    {
                        _callback?.Invoke(false, error, null);
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

            _callback?.Invoke(false, error, null);
        }

        private async void UserCancelledHook()
        {
            if (_settings.ManualCancellation) return;

            var time = DateTime.UtcNow;

            while ((DateTime.UtcNow - time).TotalSeconds < 1)
            {
                await Task.Yield();
            }

            if (_state == null) return;

            _state = null;

            const string error = "User cancelled.";

            _callback?.Invoke(false, error, null);
        }

        private void OnDeepLinkActivated(string deepLink)
        {
            Log($"Deep link activated: {deepLink}");

            deepLink = deepLink.Replace(":///", ":/"); // Some browsers may add extra slashes.

            if (_redirectUri == null || !deepLink.StartsWith(_redirectUri) || _state == null)
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
                _callback?.Invoke(false, error, null);
                return;
            }

            var state = parameters.Get("state");
            var code = parameters.Get("code");

            if (state == null || code == null) return;

            if (state == _state)
            {
                DecodeAuthResult(code);
            }
            else
            {
                Log("Unexpected response.");
            }
        }

        private void DecodeAuthResult(string code)
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(code));
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(json);

            SavedAuth = new SavedAuth(_settings.BotName, userInfo);
            SavedAuth.Save();

            if (PlayerPrefs.HasKey(TempKey))
            {
                PlayerPrefs.DeleteKey(TempKey);
                PlayerPrefs.Save();
            }

            _state = null;
            _callback?.Invoke(true, null, userInfo);
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