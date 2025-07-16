using Newtonsoft.Json;

namespace Assets.SimpleSignIn.Microsoft.Scripts
{
    public class UserInfo // https://learn.microsoft.com/ru-ru/entra/identity-platform/userinfo
    {
        [JsonProperty("sub")]
        public string Id;

        [JsonProperty("@odata.context")]
        public string Context;

        [JsonProperty("givenname")]
        public string GivenName;

        [JsonProperty("familyname")]
        public string FamilyName;

        [JsonProperty("email")]
        public string Email;

        [JsonProperty("locale")]
        public string Locale;

        [JsonProperty("picture")]
        public string Picture;
    }

}