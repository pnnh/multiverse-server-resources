
using Newtonsoft.Json;

namespace Gliese.Models
{
 
    public class OAuth2Token
    { 
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonProperty("id_token")]
        public string IdToken { get; set; } = "";

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = "";

        [JsonProperty("scope")]
        public string Scope { get; set; } = "";
    }

    public class OAuth2User {
        [JsonProperty("username")]
        public string Username { get; set; } = "";

        [JsonProperty("nickname")]
        public string Nickname { get; set; } = ""; 
    }

}
