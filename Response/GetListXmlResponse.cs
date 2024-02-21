using Newtonsoft.Json;
using System.Collections.Generic;

namespace OnlineService.Response
{
    public class GetListXmlResponse
    {
        [JsonProperty("fileList")]
        public List<XmlFileInfo> FileList { get; set; } = new List<XmlFileInfo>();
    }

    public class XmlFileInfo
    {
        [JsonProperty("fileName")]
        public string Filename { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
    }
}