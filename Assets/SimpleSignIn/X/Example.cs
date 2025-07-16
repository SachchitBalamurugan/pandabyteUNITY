using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleSignIn.X.Scripts;

namespace Assets.SimpleSignIn.X
{
    public class Example : MonoBehaviour
    {
        public XAuth XAuth;
        public Text Log;
        public Text Output;
        
        public void Start()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            XAuth = new XAuth();
            XAuth.TryResume(OnSignIn, OnGetTokenResponse);
        }

        public void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        public void SignIn()
        {
            XAuth.SignIn(OnSignIn, caching: true);
        }

        public void SignOut()
        {
            XAuth.SignOut(revokeAccessToken: true);
            Output.text = "Not signed in";
        }

        public void GetAccessToken()
        {
            XAuth.GetTokenResponse(OnGetTokenResponse);
        }

        private void OnSignIn(bool success, string error, UserInfo userInfo)
        {
            Output.text = success ? $"Hello, {userInfo.Name}!" : error;
        }

        private void OnGetTokenResponse(bool success, string error, TokenResponse tokenResponse)
        {
            Output.text = success ? $"Access token: {tokenResponse.AccessToken}" : error;
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