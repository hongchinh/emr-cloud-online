using Newtonsoft.Json;

namespace OnlineService.Request
{
    public class CreateXmlRequest : CommonRequest
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;

        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;
    }
}