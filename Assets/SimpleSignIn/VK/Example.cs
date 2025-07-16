using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleSignIn.VK.Scripts;

namespace Assets.SimpleSignIn.VK
{
    public class Example : MonoBehaviour
    {
        public Text Log;
        public Text Output;

        public void Start()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            VKAuth.OnTokenResponse += OnTokenResponse; // Optional. Subscribe to get an access token.
        }

        public void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        public void SignIn()
        {
            VKAuth.SignIn(OnSignIn);
        }

        public void SignOut()
        {
            VKAuth.SignOut();
            Output.text = "Not signed in";
        }

        private void OnSignIn(bool success, string error, UserInfo userInfo)
        {
            Output.text = success ? $"Hello, {userInfo.first_name}!" : error;
        }

        private static void OnTokenResponse(TokenResponse response)
        {
            Debug.Log($"Access token: {response.access_token}");
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