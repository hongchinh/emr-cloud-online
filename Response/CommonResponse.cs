using Newtonsoft.Json;

namespace OnlineService.Response
{
    public class CommonResponse
    {
        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        public CommonResponse(object data, string message, int status)
        {
            Data = data;
            Message = message;
            Status = status;
        }
    }
}