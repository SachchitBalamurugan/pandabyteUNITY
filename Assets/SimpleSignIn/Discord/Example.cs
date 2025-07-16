using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleSignIn.Discord.Scripts;
using UnityEngine.Networking;

namespace Assets.SimpleSignIn.Discord
{
    public class Example : MonoBehaviour
    {
        public DiscordAuth DiscordAuth;
        public Text Log;
        public Text Output;
        
        public void Start()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            DiscordAuth = new DiscordAuth();
            DiscordAuth.TryResume(OnSignIn, OnGetTokenResponse);
        }

        public void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        public void SignIn()
        {
            DiscordAuth.SignIn(OnSignIn, caching: true);
        }

        public void SignOut()
        {
            DiscordAuth.SignOut(revokeAccessToken: true);
            Output.text = "Not signed in";
        }

        public void GetAccessToken()
        {
            DiscordAuth.GetTokenResponse(OnGetTokenResponse);
        }

        private void OnSignIn(bool success, string error, UserInfo userInfo)
        {
            Output.text = success ? $"Hello, {userInfo.Name}!" : error;
        }

        private void OnGetTokenResponse(bool success, string error, TokenResponse tokenResponse)
        {
            Output.text = success ? $"Access token: {tokenResponse.AccessToken}" : error;
        }

        public void GetGuilds()
        {
            DiscordAuth.GetTokenResponse((success, error, tokenResponse) =>
            {
                if (success)
                {
                    var request = UnityWebRequest.Get("https://discord.com/api/users/@me/guilds");

                    request.SetRequestHeader("Authorization", $"Bearer {tokenResponse.AccessToken}");
                    request.SendWebRequest().completed += _ =>
                    {
                        Output.text = request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text : request.error;

                        if (request.error == "HTTP/1.1 401 Unauthorized")
                        {
                            Output.text += "\nDid you forget to add `guilds` to access scopes in DiscordAuthSettings?";
                        }
                    };
                }
                else
                {
                    Output.text = error;
                }
            });
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