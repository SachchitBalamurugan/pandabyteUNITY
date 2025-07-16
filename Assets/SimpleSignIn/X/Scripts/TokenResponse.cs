using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Assets.SimpleSignIn.X.Scripts
{
    /// <summary>
    /// Response specification:
    /// </summary>
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken;

        [JsonProperty("expires_in")]
        public int ExpiresIn;

        /// <summary>
        /// If the scope offline.access is applied an OAuth 2.0 refresh token will be issued.
        /// With this refresh token, you obtain an access token.
        /// If this scope is not passed, we will not generate a refresh token.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken;

        /// <summary>
        /// The scopes of access granted by the access_token expressed as a list of space-delimited, case-sensitive strings.
        /// </summary>
        [JsonProperty("scope")]
        public string Scope;

        /// <summary>
        /// The type of token returned. At this time, this field's value is always set to Bearer.
        /// </summary>
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