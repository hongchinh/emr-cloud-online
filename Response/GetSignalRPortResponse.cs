namespace OnlineService.Response
{
    public class GetSignalRPortResponse
    {
        public Data Data { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public int Status { get; set; }
    }

    public class Data
    {
        public SmartKarteAppSignalRPort SmartKarteAppSignalRPort { get; set; } = new();
    }

    public class SmartKarteAppSignalRPort
    {
        public int PortNumber { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
    }
}
