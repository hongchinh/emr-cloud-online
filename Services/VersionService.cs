using log4net.Appender;
using log4net;
using OnlineService.Response;
using System.Diagnostics;
using System.Reflection;
using System;
using OnlineService.Request;
using System.Collections.Generic;

namespace OnlineService.Services
{
    public class VersionService
    {
        private Handler _handler = new Handler();
        private readonly ApiService _logService = new ApiService();
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];

        public string GetAppVersion()
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(GetAppVersion);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, string.Empty),
            Constants.Logs.LogType.DEBUG));
            string? output = null;

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                var version = new GetVersionResponse
                {
                    Version = fvi.FileVersion
                };

                output = _handler.Response(version, Constants.Message.SUCCESS, 200);
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogInfo(this, methodName, Constants.Message.SUCCESS, $"FileVersion: {fvi.FileVersion}"),
                Constants.Logs.LogType.INFO));
                return output;
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                return _handler.Response(new object(), Constants.Message.Web.Error.Version.GETTING, 400);
            }
            finally
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                Constants.Logs.LogType.DEBUG));
                _logService.WriteLog(logs);
            }
        }
    }
}