using Newtonsoft.Json;
using System.Collections.Generic;

namespace OnlineService.Request
{
    public class CreateKensaIraiFileRequest : CommonRequest
    {
        [JsonProperty("kensaIraiReportItemList")]
        public List<KensaIraiFileRequest> KensaIraiReportItemList { get; set; } = new List<KensaIraiFileRequest>();

        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;


        [JsonProperty("sinDate")]
        public int SinDate { get; set; }

        [JsonProperty("raiinNo")]
        public int RaiinNo { get; set; }
    }

    public class KensaIraiFileRequest 
    {
        [JsonProperty("output")]
        public List<string> Output { get; set; } = new List<string>();

        [JsonProperty("outputDummy")]
        public List<string> OutputDummy { get; set; } = new List<string>();

        [JsonProperty("raiinInfKaId")]
        public int RaiinInfKaId { get; set; }

        [JsonProperty("kensaInfIraiCd")]
        public int KensaInfIraiCd { get; set; }

        [JsonProperty("ptInfPtNum")]
        public int PtInfPtNum { get; set; }
    }
}