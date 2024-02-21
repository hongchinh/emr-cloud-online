namespace OnlineService.Request
{
    public class UpdateSignalRPortRequest
    {
        public int PortNumber { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
    }
}
