using Newtonsoft.Json;
using System.Collections.Generic;

namespace OnlineService.Request
{
    public class MoveXmlRequest : CommonRequest
    {
        [JsonProperty("files")]
        public List<string> Files { get; set; } = new List<string>();
    }
}