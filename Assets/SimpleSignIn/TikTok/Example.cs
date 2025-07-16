using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleSignIn.TikTok.Scripts;

namespace Assets.SimpleSignIn.TikTok
{
    public class Example : MonoBehaviour
    {
        public TikTokAuth TikTokAuth;
        public Text Log;
        public Text Output;
        
        public void Start()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            TikTokAuth = new TikTokAuth();
            TikTokAuth.TryResume(OnSignIn, OnGetTokenResponse);
        }

        public void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        public void SignIn()
        {
            Output.text = "Signing in...";
            TikTokAuth.SignIn(OnSignIn, caching: true);
        }

        public void SignOut()
        {
            TikTokAuth.SignOut(revokeAccessToken: true);
            Output.text = "Not signed in";
        }

        public void GetAccessToken()
        {
            TikTokAuth.GetTokenResponse(OnGetTokenResponse);
        }

        public void RefreshAccessToken()
        {
            TikTokAuth.RefreshAccessToken(OnGetTokenResponse);
        }

        private void OnSignIn(bool success, string error, UserInfo userInfo)
        {
            Output.text = success ? $"Hello, {userInfo.DisplayName}!" : error;
        }

        private void OnGetTokenResponse(bool success, string error, TokenResponse tokenResponse)
        {
            Output.text = success ? $"Access token: {tokenResponse.AccessToken.Substring(0, 32)}..." : error;
        }

        public void Navigate(string url)
        {
            Application.OpenURL(url);
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType logType)
        {
            Log.text += condition + '\n';
        }
    }
}