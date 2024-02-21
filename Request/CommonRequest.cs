using Newtonsoft.Json;

namespace OnlineService.Request
{
    public class CommonRequest
    {
        [JsonProperty("screenCode")]
        public string ScreenCode { get; set; } = string.Empty;

        [JsonProperty("domain")]
        public string Domain { get; set; } = string.Empty;
    }
}