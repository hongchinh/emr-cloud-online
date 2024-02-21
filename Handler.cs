using log4net;
using log4net.Appender;
using Newtonsoft.Json;
using OnlineService.Response;
using System;

namespace OnlineService
{
    public class Handler
    {
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];

        public string Response(object obj, string message, int statusCode)
        {
            string methodName = nameof(Response);
            string? output = null;
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            Log.WriteLogDebug(this, methodName, "Input", $"obj: {obj}, message: {message}, statusCode: {statusCode}");

            try
            {
                var response = new CommonResponse(obj, message, statusCode);
                output = JsonConvert.SerializeObject(response, Formatting.Indented);
                return output ?? string.Empty;
            }
            catch (Exception ex)
            {
                Log.WriteLogException(this, methodName, $"Message: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                return output ?? string.Empty;
            }
            finally
            {
                Log.WriteLogDebug(this, methodName, "Output", $"Result: {output}");
            }
        }
    }
}