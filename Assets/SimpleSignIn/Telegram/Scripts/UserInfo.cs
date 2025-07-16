using Newtonsoft.Json;
using System;

namespace Assets.SimpleSignIn.Telegram.Scripts
{
    [Serializable]
    public class UserInfo
    {
        [JsonProperty("id")]
        public long Id;

        [JsonProperty("first_name")]
        public string FirstName;

        [JsonProperty("last_name")]
        public string LastName;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("photo_url")]
        public string PhotoUrl;

        [JsonProperty("auth_date")]
        public long AuthDate;

        [JsonProperty("hash")]
        public string Hash;
    }
}