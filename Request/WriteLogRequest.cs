using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineService.Request
{
    public class WriteLogRequest
    {
        public string EventCd { get; set; } = string.Empty;

        public long PtId { get; set; }

        public int SinDay { get; set; }

        public long RaiinNo { get; set; }

        public string RequestInfo { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string LogType { get; set; } = string.Empty;
        public WriteLogRequest() { }
        public WriteLogRequest(string eventCd, long ptId, int sinDay, long raiinNo, string requestInfo, string logType)
        {
            EventCd = eventCd;
            PtId = ptId;
            SinDay = sinDay;
            RaiinNo = raiinNo;
            RequestInfo = requestInfo;
            Description = string.Empty;
            LogType = logType;
        }
        public WriteLogRequest(string eventCd, string requestInfo, string logType)
        {
            EventCd = eventCd;
            PtId = 0;
            SinDay = 0;
            RaiinNo = 0;
            RequestInfo = requestInfo;
            Description = string.Empty;
            LogType = logType;
        }
    }

}
