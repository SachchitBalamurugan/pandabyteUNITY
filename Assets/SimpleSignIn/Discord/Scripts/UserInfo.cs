using Newtonsoft.Json;
using System;

namespace Assets.SimpleSignIn.Discord.Scripts
{
    [Serializable]
    public class UserInfo
    {
        [JsonProperty("id;")]
        public string Id;

        [JsonProperty("global_name")]
        public string Name;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("email")]
        public string Email;

        [JsonProperty("locale")]
        public string Locale;

        [JsonProperty("verified")]
        public bool Verified;
    }
}