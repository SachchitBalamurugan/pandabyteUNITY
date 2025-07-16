using Newtonsoft.Json;
using System;

namespace Assets.SimpleSignIn.X.Scripts
{
    [Serializable]
    public class UserInfo
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("username")]
        public string Username;

        /// <summary>
        /// NOT SUPPORTED YET: https://devcommunity.x.com/t/wheres-request-email-addresses-from-users-checkbox/167229/11
        /// The "Request email addresses from users" checkbox is available under the app permissions on developer.twitter.com.
        /// Privacy Policy URL and Terms of Service URL fields must be completed in the app settings in order for email address access to function.
        /// If enabled, users will be informed via the oauth/authorize dialog that your app can access their email address.
        /// </summary>
        [JsonProperty("email")]
        public string Email;
    }
}