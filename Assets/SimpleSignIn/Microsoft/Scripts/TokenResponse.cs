using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Assets.SimpleSignIn.Microsoft.Scripts
{
    /// <summary>
    /// Response specification: https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow#successful-response-2
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// The requested access token. The app can use this token to authenticate to the secured resource, such as a web API.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken;

        /// <summary>
        /// Indicates the token type value. The only type that Microsoft Entra ID supports is Bearer.
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType;

        /// <summary>
        /// How long the access token is valid, in seconds.
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn;

        /// <summary>
        /// The scopes that the access_token is valid for. Optional. This parameter is non-standard and, if omitted, the token is for the scopes requested on the initial leg of the flow.
        /// </summary>
        [JsonProperty("scope")]
        public string Scope;

        /// <summary>
        /// An OAuth 2.0 refresh token. The app can use this token to acquire other access tokens after the current access token expires. Refresh tokens are long-lived. They can maintain access to resources for extended periods. For more detail on refreshing an access token, refer to Refresh the access token later in this article.
        /// Note: Only provided if offline_access scope was requested.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken;

        /// <summary>
        /// A JSON Web Token. The app can decode the segments of this token to request information about the user who signed in. The app can cache the values and display them, and confidential clients can use this token for authorization. For more information about id_tokens, see the id_token reference.
        /// Note: Only provided if openid scope was requested.
        /// </summary>
        [JsonProperty("id_token")]
        public string IdToken;

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