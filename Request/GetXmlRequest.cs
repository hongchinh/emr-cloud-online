using Newtonsoft.Json;

namespace OnlineService.Request
{
    public class GetXmlRequest : CommonRequest
    {
        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;
    }
}
