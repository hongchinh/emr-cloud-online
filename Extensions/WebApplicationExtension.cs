using log4net;
using log4net.Appender;
using Microsoft.AspNetCore.Builder;
using System;
using System.Linq;
using System.Reflection;

namespace OnlineService.Extensions
{
    public static class WebApplicationExtension
    {
        private static readonly FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        private static readonly string _className = $"{Assembly.GetCallingAssembly().GetName().Name}.{nameof(WebApplicationExtension)}";

        public static string GetBaseUrl(this WebApplication webApplication)
        {
            string methodName = nameof(GetBaseUrl);
            string? output = null;
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            Log.WriteLogDebug(_className, methodName, "Input", $"webApplication: {webApplication}");

            try
            {
                if (webApplication == null)
                {
                    Log.WriteLogError(_className, methodName, "webApplication is null", $"webApplication: {webApplication}");
                    return output ?? string.Empty;
                }

                output = webApplication.Urls.FirstOrDefault(x => x.StartsWith("https"));
                return output ?? string.Empty;
            }
            catch (Exception ex)
            {
                Log.WriteLogException(_className, methodName, $"Message: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                return output ?? string.Empty;
            }
            finally
            {
                Log.WriteLogDebug(_className, methodName, "Output", $"Result: {output}");
            }
        }
    }
}