using Newtonsoft.Json;

namespace Assets.SimpleSignIn.TikTok.Scripts
{
    public class UserInfo // https://developers.tiktok.com/doc/tiktok-api-v2-get-user-info
    {
        [JsonProperty("open_id")]
        public string Id;

        [JsonProperty("display_name")]
        public string DisplayName;

        [JsonProperty("avatar_url")]
        public string AvatarUrl;

        [JsonProperty("union_id")]
        public string UnionId;
    }

}