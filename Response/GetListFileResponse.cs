using Newtonsoft.Json;
using OnlineService.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineService.Response
{
    public class GetListFileResponse
    {
        [JsonProperty("fileList")]
        public List<FileData> FileList { get; set; } = new List<FileData>();
    }

    public class FileData
    {
        [JsonProperty("fileName")]
        public string Filename { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
        [JsonProperty("contentFile")]
        public string ContentFile { get; set; } = string.Empty;

        [JsonProperty("contentBase64")]
        public string ContentBase64 { get; set; } = string.Empty;

        [JsonProperty("typeFile")]
        public TypeFileEnum TypeFile { get; set; }
    }
}
