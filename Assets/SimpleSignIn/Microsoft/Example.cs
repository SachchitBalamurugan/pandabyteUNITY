using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleSignIn.Microsoft.Scripts;

namespace Assets.SimpleSignIn.Microsoft
{
    public class Example : MonoBehaviour
    {
        public MicrosoftAuth MicrosoftAuth;
        public Text Log;
        public Text Output;
        
        public void Start()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            MicrosoftAuth = new MicrosoftAuth();
            MicrosoftAuth.TryResume(OnSignIn, OnGetTokenResponse);
        }

        public void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        public void SignIn()
        {
            Output.text = "Signing in...";
            MicrosoftAuth.SignIn(OnSignIn, caching: true);
        }

        public void SignOut()
        {
            MicrosoftAuth.SignOut(logout: true);
            Output.text = "Not signed in";
        }

        public void GetAccessToken()
        {
            MicrosoftAuth.GetTokenResponse(OnGetTokenResponse);
        }

        private void OnSignIn(bool success, string error, UserInfo userInfo)
        {
            Output.text = success ? $"Hello, {userInfo.GivenName} {userInfo.FamilyName}!" : error;
        }

        private void OnGetTokenResponse(bool success, string error, TokenResponse tokenResponse)
        {
            Output.text = success ? $"Access token: {tokenResponse.AccessToken.Substring(0, 32)}..." : error;

            if (!success) return;

            var jwt = new JWT(tokenResponse.IdToken);

            Debug.Log($"JSON Web Token (JWT) Payload: {jwt.Payload}");
            
            jwt.ValidateSignature(MicrosoftAuth.ClientId, OnValidateSignature);
        }

        private void OnValidateSignature(bool success, string error)
        {
            Output.text += Environment.NewLine;
            Output.text += success ? "JWT signature validated" : error;
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