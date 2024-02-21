using Newtonsoft.Json;

namespace OnlineService.Response
{
    public class GetVersionResponse
    {
        [JsonProperty("version")]
        public string? Version { get; set; }
    }
}