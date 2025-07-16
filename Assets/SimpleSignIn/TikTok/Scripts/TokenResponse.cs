using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Assets.SimpleSignIn.TikTok.Scripts
{
    /// <summary>
    /// Response specification: https://developers.tiktok.com/doc/oauth-user-access-token-management
    /// </summary>
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken;

        [JsonProperty("expires_in")]
        public int ExpiresIn;

        [JsonProperty("refresh_token")]
        public string RefreshToken;

        [JsonProperty("scope")]
        public string Scope;

        [JsonProperty("token_type")]
        public string TokenType;

        /// <summary>
        /// This aux property is calculated by the asset.
        /// </summary>
        public DateTime Expiration;

        public bool Expired => Expiration < DateTime.UtcNow;

        [Preserve]
        private TokenResponse()
        {
        }

        public static TokenResponse Parse(string json)
        {
            var response = JsonConvert.DeserializeObject<TokenResponse>(json);

            response.Expiration = DateTime.UtcNow.AddSeconds(response.ExpiresIn - 10);

            return response;
        }
    }
}