using log4net;
using log4net.Appender;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using OnlineService.Base;
using OnlineService.Request;
using OnlineService.Response;
using OnlineService.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace OnlineService.Services
{
    public class XmlFileService
    {
        private readonly Handler _handler = new Handler();
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        private bool flag;
        private CancellationToken token;
        private readonly ApiService _logService = new ApiService();

        private class WatcherResult
        {
            public WaitForChangedResult ChangedResult { get; set; }

            public string Path { get; set; } = string.Empty;
        }

        private class PathConf
        {
            public int GrpCd { get; set; }

            public int SeqNo { get; set; }

            public string Path { get; set; } = string.Empty;
        }

        public string GetListXmlFile(GetXmlRequest request, string endpoint)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(GetListXmlFile);
            GetListXmlResponse? response = null;

            try
            {
                Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

                if (request == null)
                {
                    logs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.REQUEST_NULL, $"request: {nameof(GetXmlRequest)}"),
                    Constants.Logs.LogType.ERROR));
                    return _handler.Response(new object(), Constants.Message.Web.Error.File.REQUEST_NULL, 400);
                }
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"request.ScreenCode: {request.ScreenCode}, request.Domain: {request.Domain}, request.Token: {request.Token}, endpoint: {endpoint}"),
                Constants.Logs.LogType.DEBUG));

                switch (request.ScreenCode)
                {
                    case Constants.ScreenCode.VISITING:
                        var listXmlPath = GetListXmlPath(Constants.PathConf.GRPCD_103, endpoint, request.Domain, request.Token, out string message);

                        Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

                        if (!string.IsNullOrEmpty(message))
                        {
                            if (message.Contains(Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT_ENG))
                            {
                                logs.Add(new WriteLogRequest(
                                Constants.Logs.EventCd.SIGNALR,
                                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                                Constants.Logs.LogType.DEBUG));
                                return _handler.Response(new GetListXmlResponse(), Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT, 400);
                            }

                            if (message.Contains(Constants.Message.Web.Error.Api.INVALID_TOKEN))
                            {
                                logs.Add(new WriteLogRequest(
                                Constants.Logs.EventCd.SIGNALR,
                                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                                Constants.Logs.LogType.DEBUG));
                                logs.Add(new WriteLogRequest(
                                Constants.Logs.EventCd.SIGNALR,
                                Log.WriteLogError(this, methodName, message),
                                Constants.Logs.LogType.ERROR));
                                return _handler.Response(new object(), message, 401);
                            }
                        }

                        if (!listXmlPath.Any())
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.EMPTY_LIST),
                            Constants.Logs.LogType.ERROR));
                            return _handler.Response(new GetListXmlResponse(), Constants.Message.Web.Error.Path.EMPTY, 400); ;
                        }

                        foreach (var xmlPath in listXmlPath)
                        {
                            if (!Directory.Exists(xmlPath))
                            {
                                logs.Add(new WriteLogRequest(
                                Constants.Logs.EventCd.SIGNALR,
                                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                                Constants.Logs.LogType.DEBUG));
                                logs.Add(new WriteLogRequest(
                                Constants.Logs.EventCd.SIGNALR,
                                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.NOT_EXIST, $"xmlPath: {xmlPath}"),
                                Constants.Logs.LogType.ERROR));
                                return _handler.Response(new GetListXmlResponse(), Constants.Message.Web.Error.Path.NOT_EXIST, 400); ;
                            }
                        }

                        for (int i = 0; i < listXmlPath.Count; i++)
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogInfo(this, methodName, Constants.Message.Log.Info.SignalR.Path.LIST, $"Path {i}: {listXmlPath[i]}"),
                            Constants.Logs.LogType.INFO));
                        }

                        var result = new List<GetListXmlResponse>();

                        foreach (var xmlPath in listXmlPath)
                        {
                            var listFile = GetListXmlFileResponse(xmlPath);
                            if (listFile != null || listFile?.FileList != null)
                            {
                                result.Add(listFile);
                            }
                        }

                        if (result.Any())
                        {
                            response = new GetListXmlResponse();
                            foreach (var getListXmlResponse in result)
                            {
                                if (getListXmlResponse.FileList.Any())
                                {
                                    foreach (var xmlFile in getListXmlResponse.FileList)
                                    {
                                        response.FileList.Add(xmlFile);
                                    }
                                }
                            }

                            var output = _handler.Response(response, Constants.Message.SUCCESS, 200);
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogInfo(this, methodName, Constants.Message.SUCCESS, $"Result: {result.Count} file(s)"),
                            Constants.Logs.LogType.INFO));
                            return output;
                        }
                        break;

                    default:
                        break;
                }
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                Constants.Logs.LogType.DEBUG));
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.ScreenCode.INVALID, $"Screen Code: {request.ScreenCode}"),
                Constants.Logs.LogType.ERROR));
                return _handler.Response(new GetListXmlResponse(), Constants.Message.Web.Error.ScreenCode.INVALID, 400);
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                Constants.Logs.LogType.DEBUG));
                return _handler.Response(new GetListXmlResponse(), Constants.Message.Web.Error.File.GETTING, 400);
            }
            finally
            {
                _logService.WriteLog(logs);
            }
        }

        public void DetectXmlFile(GetXmlRequest request, string endpoint, int timeout, string username, string password, IHubCallerClients clients, CancellationToken cancellationToken)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(DetectXmlFile);
            string? response = null;
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

            if (request == null)
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.REQUEST_NULL, $"request: {nameof(GetXmlRequest)}"),
                Constants.Logs.LogType.ERROR));
                response = _handler.Response(new object(), Constants.Message.Web.Error.File.REQUEST_NULL, 400);
                clients.Caller.SendAsync(MethodList.DetectXmlFile, response).Wait();
                _logService.WriteLog(logs);
                return;
            }
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"request.ScreenCode: {request.ScreenCode}, request.Domain: {request.Domain}, request.Token: {request.Token}, endpoint: {endpoint}, timeout: {timeout}"),
            Constants.Logs.LogType.DEBUG));

            switch (request.ScreenCode)
            {
                case Constants.ScreenCode.VISITING:
                case Constants.ScreenCode.PATIENT_INFO:
                    var listXmlPath = GetListXmlPath(Constants.PathConf.GRPCD_103, endpoint, request.Domain, request.Token, out string message);

                    Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

                    if (!string.IsNullOrEmpty(message))
                    {
                        if (message.Contains(Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT_ENG))
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, message),
                            Constants.Logs.LogType.ERROR));
                            response = _handler.Response(new GetListXmlResponse(), Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT, 400);
                            clients.Caller.SendAsync(MethodList.DetectXmlFile, response).Wait();
                            _logService.WriteLog(logs);
                            return;
                        }

                        if (message.Contains(Constants.Message.Web.Error.Api.INVALID_TOKEN))
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, message),
                            Constants.Logs.LogType.ERROR));
                            response = _handler.Response(new object(), message, 401);
                            clients.Caller.SendAsync(MethodList.DetectXmlFile, response).Wait();
                            _logService.WriteLog(logs);
                            return;
                        }
                    }

                    if (!listXmlPath.Any())
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.EMPTY_LIST),
                        Constants.Logs.LogType.ERROR));
                        response = _handler.Response(new object(), Constants.Message.Web.Error.Path.EMPTY, 400);
                        clients.Caller.SendAsync(MethodList.DetectXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }

                    foreach (var xmlPath in listXmlPath)
                    {
                        if (!Directory.Exists(xmlPath))
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.NOT_EXIST, $"xmlPath: {xmlPath}"),
                            Constants.Logs.LogType.ERROR));
                            response = _handler.Response(new object(), Constants.Message.Web.Error.Path.NOT_EXIST, 400);
                            clients.Caller.SendAsync(MethodList.DetectXmlFile, response).Wait();
                            _logService.WriteLog(logs);
                            return;
                        }
                    }

                    for (int i = 0; i < listXmlPath.Count; i++)
                    {
                        logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogInfo(this, methodName, Constants.Message.Log.Info.SignalR.Path.LIST, $"Path {i}: {listXmlPath[i]}"),
                            Constants.Logs.LogType.INFO));
                    }

                    var tasks = new List<Task>();

                    foreach (var xmlPath in listXmlPath)
                    {
                        var task = Task.Factory.StartNew(() => SendXmlResponse(xmlPath, username, password, clients, timeout, cancellationToken));
                        tasks.Add(task);
                    }
                    Task.WhenAny(tasks).Wait();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        response = _handler.Response(new object(), "The process has been aborted", 400);
                        clients.Caller.SendAsync(MethodList.DetectXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }
                    if (!flag)
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        response = _handler.Response(new object(), Constants.Message.Web.Error.File.NOT_DETECTED, 400);
                        clients.Caller.SendAsync(MethodList.DetectXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }
                    logs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {Constants.Message.SUCCESS}"),
                    Constants.Logs.LogType.DEBUG));
                    _logService.WriteLog(logs);
                    return;

                default:
                    break;
            }

            response = _handler.Response(new object(), Constants.Message.Web.Error.ScreenCode.INVALID, 400);
            clients.Caller.SendAsync(MethodList.DetectXmlFile, response).Wait();
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
            Constants.Logs.LogType.DEBUG));
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.ScreenCode.INVALID, $"Screen Code: {request.ScreenCode}"),
            Constants.Logs.LogType.ERROR));
            _logService.WriteLog(logs);
            return;
        }

        public string MoveXmlFile(MoveXmlRequest request)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(MoveXmlFile);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

            if (request == null)
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.REQUEST_NULL, $"request: {nameof(MoveXmlRequest)}"),
                Constants.Logs.LogType.ERROR));
                return _handler.Response(new object(), Constants.Message.Web.Error.File.REQUEST_NULL, 400);
            }
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"request.ScreenCode: {request.ScreenCode}, request.Domain: {request.Domain}, request.Files.Count: {request.Files.Count}"),
            Constants.Logs.LogType.DEBUG));

            string? output = null;

            if (request.Files == null || !request.Files.Any())
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                Constants.Logs.LogType.DEBUG));
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.EMPTY_LIST),
                Constants.Logs.LogType.ERROR));
                return _handler.Response(new object(), Constants.Message.Web.Error.File.EMPTY_LIST, 400);
            }

            var duplicates = request.Files.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);

            if (duplicates.Any())
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                Constants.Logs.LogType.DEBUG));
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.DUPLICATE_FILE, $"duplicates: {duplicates.Count()}"),
                Constants.Logs.LogType.ERROR));
                return _handler.Response(new object(), Constants.Message.Web.Error.File.DUPLICATE_FILE, 400);
            }

            switch (request.ScreenCode)
            {
                case Constants.ScreenCode.VISITING:
                case Constants.ScreenCode.PATIENT_INFO:
                case Constants.ScreenCode.RECEPTION:
                case Constants.ScreenCode.MAIN_MENU:
                    foreach (var file in request.Files)
                    {
                        if (File.Exists(file))
                        {
                            try
                            {
                                var logPath = AppDomain.CurrentDomain.BaseDirectory + $@"logs\{Constants.JAPAN_TIME:yyyyMMdd}\online_qualification\";

                                if (!Directory.Exists(logPath))
                                {
                                    Directory.CreateDirectory(logPath);
                                }

                                var sourceFilePath = file;
                                var index = file.Trim().LastIndexOf("\\");
                                var fileName = file.Substring(index + 1);
                                var destFilePath = Path.Combine(logPath, fileName);

                                File.Move(sourceFilePath, destFilePath, true);
                            }
                            catch (Exception ex)
                            {
                                logs.Add(new WriteLogRequest(
                                Constants.Logs.EventCd.SIGNALR,
                                Log.WriteLogException(this, methodName, $"Exception: {ex.Message}", $"file: {file}", $"Stack Trace: {ex.StackTrace ?? string.Empty}"),
                                Constants.Logs.LogType.EXCEPTION));
                                return _handler.Response(new object(), Constants.Message.Web.Error.File.MOVING, 400);
                            }
                        }
                    }

                    output = _handler.Response(new object(), Constants.Message.SUCCESS, 200);
                    logs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                    Constants.Logs.LogType.DEBUG));
                    logs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogInfo(this, methodName, Constants.Message.SUCCESS, $"Result: {request.Files} file(s)"),
                    Constants.Logs.LogType.INFO));
                    return output;

                default:
                    break;
            }
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
            Constants.Logs.LogType.DEBUG));
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.ScreenCode.INVALID, $"Screen Code: {request.ScreenCode}"),
            Constants.Logs.LogType.ERROR));
            return _handler.Response(new object(), Constants.Message.Web.Error.ScreenCode.INVALID, 400);
        }

        public void CreateXmlFile(CreateXmlRequest request, string endpoint, string username, string password, IHubCallerClients clients, int timeout, CancellationToken cancellationToken)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(CreateXmlFile);
            string? response = null;
            string? responseFolder = string.Empty;
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

            if (request == null)
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.REQUEST_NULL, $"request: {nameof(CreateXmlRequest)}"),
                Constants.Logs.LogType.ERROR));
                response = _handler.Response(new object(), Constants.Message.Web.Error.File.REQUEST_NULL, 400);
                clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                _logService.WriteLog(logs);
                return;
            }
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"request.ScreenCode: {request.ScreenCode}, request.Domain: {request.Domain}, request.Token: {request.Token}, request.FileName: {request.FileName}, request.Content: {request.Content}, endpoint: {endpoint}"),
            Constants.Logs.LogType.DEBUG));
            var pathRequest = string.Empty;
            switch (request.ScreenCode)
            {
                case Constants.ScreenCode.PATIENT_INFO:
                case Constants.ScreenCode.RECEPTION:
                case Constants.ScreenCode.MAIN_MENU:
                    string message;
                    var requestFolders = GetListXmlPath(Constants.PathConf.GRPCD_101, endpoint, request.Domain, request.Token, out message);

                    Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

                    if (!string.IsNullOrEmpty(message))
                    {
                        if (message.Contains(Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT_ENG))
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, message),
                            Constants.Logs.LogType.ERROR));
                            response = _handler.Response(new GetListXmlResponse(), Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT, 400);
                            clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                            _logService.WriteLog(logs);
                            return;
                        }

                        if (message.Contains(Constants.Message.Web.Error.Api.INVALID_TOKEN))
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, message),
                            Constants.Logs.LogType.ERROR));
                            response = _handler.Response(new object(), message, 401);
                            clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                            _logService.WriteLog(logs);
                            return;
                        }
                    }

                    var responseFolders = GetListXmlPath(Constants.PathConf.GRPCD_102, endpoint, request.Domain, request.Token, out message);

                    if (!string.IsNullOrEmpty(message))
                    {
                        if (message.Contains(Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT_ENG))
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                            Constants.Logs.LogType.DEBUG));
                            response = _handler.Response(new GetListXmlResponse(), Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT, 400);
                            clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                            _logService.WriteLog(logs);
                            return;
                        }

                        if (message.Contains(Constants.Message.Web.Error.Api.INVALID_TOKEN))
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, message),
                            Constants.Logs.LogType.ERROR));
                            response = _handler.Response(new object(), message, 401);
                            clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                            _logService.WriteLog(logs);
                            return;
                        }
                    }

                    var requestFolder = requestFolders?.FirstOrDefault();
                    responseFolder = responseFolders?.FirstOrDefault();

                    if (string.IsNullOrEmpty(requestFolder))
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.REQ_EMPTY),
                        Constants.Logs.LogType.ERROR));
                        response = _handler.Response(new object(), Constants.Message.Web.Error.Path.EMPTY, 400);
                        clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }

                    if (string.IsNullOrEmpty(responseFolder))
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                         Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.RES_EMPTY),
                        Constants.Logs.LogType.ERROR));
                        response = _handler.Response(new object(), Constants.Message.Web.Error.Path.EMPTY, 400);
                        clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }

                    if (!Directory.Exists(requestFolder))
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.REQ_NOT_EXIST, $"requestFolder: {requestFolder}"),
                        Constants.Logs.LogType.ERROR));
                        response = _handler.Response(new object(), Constants.Message.Web.Error.Path.NOT_EXIST, 400);
                        clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }

                    if (!Directory.Exists(responseFolder))
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.RES_NOT_EXIST, $"responseFolder: {responseFolder}"),
                        Constants.Logs.LogType.ERROR));
                        response = _handler.Response(new object(), Constants.Message.Web.Error.Path.NOT_EXIST, 400);
                        clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }

                    if (string.IsNullOrEmpty(request.FileName))
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.EMPTY_NAME),
                        Constants.Logs.LogType.ERROR));
                        response = _handler.Response(new object(), Constants.Message.Web.Error.File.EMPTY_NAME, 400);
                        clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }
                    var listRequest = new List<string> { "OQSsiquc01req_", "OQSmuquc01req_", "OQSmuquc02req_" };
                    if (!listRequest.Any(i => request.FileName.StartsWith(i)) ||
                        !Path.GetExtension(request.FileName).Equals(".xml"))
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.WRONG_FORMAT_NAME, $"file: {request.FileName}"),
                        Constants.Logs.LogType.ERROR));
                        response = _handler.Response(new object(), Constants.Message.Web.Error.File.WRONG_FORMAT_NAME, 400);
                        clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }

                    var isCreated = CreateNewXmlFile(requestFolder, request.FileName, request.Content);

                    if (!isCreated)
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.CREATING),
                        Constants.Logs.LogType.ERROR));
                        response = _handler.Response(new object(), Constants.Message.Web.Error.File.CREATING, 400);
                        clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }

                    var requestFile = Path.Combine(requestFolder, request.FileName);
                    pathRequest = requestFile;
                    var requestComparation = string.Empty;
                    if (request.FileName.StartsWith("OQSmuquc02req_"))
                    {
                        requestComparation = GetRequestComparationReceptionNumber(requestFile);
                    }
                    else
                    {
                        requestComparation = GetRequestComparation(requestFile);
                    }

                    if (string.IsNullOrEmpty(requestComparation))
                    {
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.GETTING, $"requestComparation: {requestComparation}"),
                        Constants.Logs.LogType.ERROR));
                        response = _handler.Response(new object(), Constants.Message.Web.Error.File.GETTING, 400);
                        clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }

                    token = cancellationToken;
                    SendXmlResponse(responseFolder, requestComparation, username, password, clients, timeout, token);
                    break;

                default:
                    break;
            }
            if (token.IsCancellationRequested || !flag)
            {
                FileUtil.DeleteFile(pathRequest);
                if (token.IsCancellationRequested)
                {
                    logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                Constants.Logs.LogType.DEBUG));
                    response = _handler.Response(new object(), "The process has been aborted", 400);
                    clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                    _logService.WriteLog(logs);
                    return;
                }
                if (!flag)
                {
                    logs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                    Constants.Logs.LogType.DEBUG));
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR, Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.NOT_DETECTED, $"responseFolder: {responseFolder}"),
                        Constants.Logs.LogType.ERROR));
                    response = _handler.Response(new object(), Constants.Message.Web.Error.File.NOT_DETECTED, 400);
                    clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                    _logService.WriteLog(logs);
                    return;
                }
            }
            _logService.WriteLog(logs);
        }

        public string CreateXmlFileUpdateRefNo(CreateXmlRequest request, string endpoint)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(CreateXmlFileUpdateRefNo);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

            if (request == null)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.REQUEST_NULL, $"request: {nameof(CreateXmlRequest)}"),
                    Constants.Logs.LogType.ERROR));
                return _handler.Response(new object(), Constants.Message.Web.Error.File.REQUEST_NULL, 400);
            }
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"request.ScreenCode: {request.ScreenCode}, request.Domain: {request.Domain}, request.Token: {request.Token}, request.FileName: {request.FileName}, request.Content: {request.Content}, endpoint: {endpoint}"),
                    Constants.Logs.LogType.DEBUG));

            string? output = null;

            switch (request.ScreenCode)
            {
                case Constants.ScreenCode.VISITING:
                case Constants.ScreenCode.PATIENT_INFO:
                case Constants.ScreenCode.RECEPTION:
                    var requestFolders = GetListXmlPath(Constants.PathConf.GRPCD_101, endpoint, request.Domain, request.Token, out string message);

                    Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

                    if (!string.IsNullOrEmpty(message))
                    {
                        if (message.Contains(Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT_ENG))
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, message),
                            Constants.Logs.LogType.ERROR));
                            return _handler.Response(new object(), Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT, 400);
                        }

                        if (message.Contains(Constants.Message.Web.Error.Api.INVALID_TOKEN))
                        {
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, message),
                            Constants.Logs.LogType.ERROR));
                            return _handler.Response(new object(), message, 401);
                        }
                    }

                    var requestFolder = requestFolders?.FirstOrDefault();

                    if (string.IsNullOrEmpty(requestFolder))
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.REQ_EMPTY),
                        Constants.Logs.LogType.ERROR));
                        return _handler.Response(new object(), Constants.Message.Web.Error.Path.EMPTY, 400);
                    }

                    if (!Directory.Exists(requestFolder))
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.REQ_NOT_EXIST, $"requestFolder: {requestFolder}"),
                        Constants.Logs.LogType.ERROR));
                        return _handler.Response(new object(), Constants.Message.Web.Error.Path.NOT_EXIST, 400);
                    }

                    if (string.IsNullOrEmpty(request.FileName))
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.EMPTY_NAME),
                        Constants.Logs.LogType.ERROR));
                        return _handler.Response(new object(), Constants.Message.Web.Error.File.EMPTY_NAME, 400);
                    }

                    if (!request.FileName.StartsWith("OQSsiimm01req_") || !Path.GetExtension(request.FileName).Equals(".xml"))
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.WRONG_FORMAT_NAME, $"file: {request.FileName}"),
                        Constants.Logs.LogType.ERROR));
                        return _handler.Response(new object(), Constants.Message.Web.Error.File.WRONG_FORMAT_NAME, 400);
                    }

                    var isCreated = CreateNewXmlFile(requestFolder, request.FileName, request.Content);

                    if (!isCreated)
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.CREATING),
                        Constants.Logs.LogType.ERROR));
                        return _handler.Response(new object(), Constants.Message.Web.Error.File.CREATING, 400);
                    }

                    output = _handler.Response(new object(), Constants.Message.SUCCESS, 200);
                    Log.WriteLogInfo(this, methodName, Constants.Message.SUCCESS, $"isCreated: {isCreated}");
                    return output;

                default:
                    break;
            }
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
            Constants.Logs.LogType.DEBUG));
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.ScreenCode.INVALID, $"Screen Code: {request.ScreenCode}"),
            Constants.Logs.LogType.ERROR));
            return _handler.Response(new object(), Constants.Message.Web.Error.ScreenCode.INVALID, 400);
        }

        public string CreateKensaIraiFile(CreateKensaIraiFileRequest request, string endpoint)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(CreateKensaIraiFile);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

            if (request == null)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.REQUEST_NULL, $"request: {nameof(KensaIraiFileRequest)}"),
                Constants.Logs.LogType.ERROR));
                _logService.WriteLog(logs);
                return _handler.Response(new object(), Constants.Message.Web.Error.File.REQUEST_NULL, 400);
            }
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"request.ScreenCode: {request.ScreenCode}, request.Domain: {request.Domain}, request.Token: {request.Token}, request.ListKensaIrai.Count: {request.KensaIraiReportItemList.Count}"),
                Constants.Logs.LogType.DEBUG));

            if (!request.KensaIraiReportItemList.Any())
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.EMPTY_LIST, $"request.ListKensaIrai.Count: {request.KensaIraiReportItemList.Count}"),
                Constants.Logs.LogType.ERROR));
                _logService.WriteLog(logs);
                return _handler.Response(new object(), Constants.Message.Web.Error.File.EMPTY_LIST, 400);
            }

            string? output = null;
            bool isSuccess = false;

            switch (request.ScreenCode)
            {
                case Constants.ScreenCode.MEDICAL:
                    var paths = GetListXmlPath(Constants.PathConf.GRPCD_54, endpoint, request.Domain, request.Token, out string message, true);

                    Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

                    if (!string.IsNullOrEmpty(message))
                    {
                        if (message.Contains(Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT_ENG))
                        {
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, message),
                            Constants.Logs.LogType.ERROR));
                            _logService.WriteLog(logs);
                            return _handler.Response(new object(), Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT, 400);
                        }
                        else if (message.Contains(Constants.Message.Web.Error.Api.INVALID_TOKEN))
                        {
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, message),
                            Constants.Logs.LogType.ERROR));
                            _logService.WriteLog(logs);
                            return _handler.Response(new object(), message, 401);
                        }
                    }

                    if (!paths.Any())
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                        Constants.Logs.LogType.DEBUG));
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.EMPTY_LIST),
                        Constants.Logs.LogType.ERROR));
                        _logService.WriteLog(logs);
                        return _handler.Response(new GetListXmlResponse(), Constants.Message.Web.Error.Path.EMPTY, 400); ;
                    }

                    for (int i = 0; i < paths.Count; i++)
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogInfo(this, methodName, Constants.Message.Log.Info.SignalR.Path.LIST, $"Path {i}: {paths[i]}"),
                        Constants.Logs.LogType.INFO));
                    }

                    foreach (var item in request.KensaIraiReportItemList)
                    {
                        var isCreated = IsKensaIraiCreated(paths, item, request.SinDate, request.RaiinNo);

                        if (!isCreated)
                        {
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.CREATING),
                            Constants.Logs.LogType.ERROR));
                            isSuccess = false;
                        }
                        else
                        {
                            isSuccess = true;
                        }
                    }

                    if (isSuccess)
                    {
                        output = _handler.Response(new object(), Constants.Message.SUCCESS, 200);
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogInfo(this, methodName, Constants.Message.SUCCESS, $"isSuccess: {isSuccess}"),
                        Constants.Logs.LogType.INFO));
                        _logService.WriteLog(logs);
                        return output;
                    }

                    output = _handler.Response(new object(), Constants.Message.Web.Error.File.CREATING, 400);
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.CREATING, $"isSuccess: {isSuccess}"),
                        Constants.Logs.LogType.ERROR));
                    _logService.WriteLog(logs);
                    return output;

                default:
                    break;
            }
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
            Constants.Logs.LogType.DEBUG));
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.ScreenCode.INVALID, $"Screen Code: {request.ScreenCode}"),
            Constants.Logs.LogType.ERROR));
            _logService.WriteLog(logs);
            return _handler.Response(new object(), Constants.Message.Web.Error.ScreenCode.INVALID, 400);
        }

        private GetListXmlResponse? GetListXmlFileResponse(string xmlFolderPath)
        {
            var extensionList = new List<string> { "xml" };
            var xmlFileList = Directory
                .EnumerateFiles(xmlFolderPath, "OQSsiquc01res_*.xml", SearchOption.AllDirectories)
                .Where(s => extensionList.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            if (xmlFileList.Any())
            {
                GetListXmlResponse getXmlResponse = new GetListXmlResponse();
                foreach (var xmlFile in xmlFileList)
                {
                    var content = File.ReadAllText(xmlFile);
                    getXmlResponse.FileList.Add(new XmlFileInfo()
                    {
                        Content = content,
                        Filename = xmlFile
                    });
                }
                return getXmlResponse;
            }
            return new GetListXmlResponse();
        }

        private XmlFileInfo? GetXmlFileInfo(string xmlFile, string username, string password)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(GetXmlFileInfo);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"xmlFile: {xmlFile}");
            XmlFileInfo? output = null;

            try
            {
                if (!File.Exists(xmlFile))
                {
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.NOT_EXIST, $"file: {xmlFile}"),
                    Constants.Logs.LogType.ERROR));
                    return output;
                }

                var fileInfo = new FileInfo(xmlFile);

                if (IsFileLocked(fileInfo))
                {
                    var isUnlocked = Unlocked(xmlFile);
                    if (!isUnlocked)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                }
                //The temporary kill file is ignored
                //if (IsFileLocked(fileInfo))
                //{
                //    List<Process> lstProcs = new List<Process>();
                //    lstProcs = ProcessHandler.WhoIsLocking(xmlFile);

                //    foreach (Process p in lstProcs)
                //    {
                //        if (p.MachineName == ".")
                //            ProcessHandler.LocalProcessKill(p.ProcessName);
                //        else
                //            ProcessHandler.RemoteProcessKill(p.MachineName, username, password, p.ProcessName);
                //    }
                //}

                string content = string.Empty;
                using (var fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }

                output = new XmlFileInfo
                {
                    Content = content,
                    Filename = xmlFile
                };
                return output;
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                return output;
            }
            finally
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                Constants.Logs.LogType.DEBUG));
                _logService.WriteLog(logs);
            }
        }

        private List<string> GetListXmlPath(int type, string endpoint, string domain, string token, out string message, bool isKensaIrai = false)
        {
            string methodName = nameof(GetListXmlPath);
            message = string.Empty;
            Log.SwitchFileAppender(_appender, Constants.Logs.API);
            List<string>? result = null;
            var logs = new List<WriteLogRequest>();
            try
            {
                var machine = Environment.MachineName;
                endpoint = string.Format(endpoint, type, machine, isKensaIrai);
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", token);
                client.DefaultRequestHeaders.Add("Domain", domain);
                HttpResponseMessage response = client.GetAsync(endpoint).Result;
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.API,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"type: {type}, endpoint: {endpoint}, domain: {domain}, token: {token}, isKensaIrai: {isKensaIrai}"),
                Constants.Logs.LogType.DEBUG));
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.API,
                Log.WriteLogInfo(this, methodName, $"Request from Machine: {machine}", $"Status Code: {response.StatusCode}"),
                Constants.Logs.LogType.INFO));

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var objectType = new
                    {
                        Data = new
                        {
                            SystemConfListXmlPath = new List<object> { }
                        }
                    };
                    var obj = JsonConvert.DeserializeAnonymousType(json, objectType);

                    if (obj == null || obj?.Data == null || obj?.Data?.SystemConfListXmlPath == null || obj?.Data?.SystemConfListXmlPath?.Count == 0)
                        return new List<string>();

                    result = new List<string>();
                    var listPathConf = new List<PathConf>();

                    foreach (var item in obj.Data.SystemConfListXmlPath)
                    {
                        var pathConf = JsonConvert.DeserializeObject<PathConf>(item?.ToString() ?? string.Empty);
                        if (pathConf != null)
                            listPathConf.Add(pathConf);
                    }

                    if (listPathConf.Any())
                    {
                        switch (type)
                        {
                            case Constants.PathConf.GRPCD_101:
                            case Constants.PathConf.GRPCD_102:
                                var minSeq = listPathConf.Min(x => x.SeqNo);
                                result = listPathConf.Where(x => x.SeqNo == minSeq).Select(x => x.Path).ToList();
                                break;

                            case Constants.PathConf.GRPCD_103:
                            case Constants.PathConf.GRPCD_54:
                                result = listPathConf.Select(x => x.Path).ToList();
                                break;

                            default:
                                break;
                        }
                    }

                    return result;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    message = Constants.Message.Web.Error.Api.INVALID_TOKEN;
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.API,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Api.INVALID_TOKEN, $"token: {token}"),
                    Constants.Logs.LogType.ERROR));
                    return new List<string>();
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.API,
                    Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), $"type: {type}, endpoint: {endpoint}, domain: {domain}, token: {token}, message: {message}, isKensaIrai: {isKensaIrai}", string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                    Constants.Logs.LogType.EXCEPTION));
                message = ex.Message;
                return new List<string>();
            }
            finally
            {
                if (result?.Count > 0)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.API,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Path {i}: {result[i]}"),
                        Constants.Logs.LogType.DEBUG));
                    }
                }
                else
                {
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.API,
                    Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {result}"),
                    Constants.Logs.LogType.DEBUG));
                }
                _logService.WriteLog(logs);
            }
        }

        private bool IsFileLocked(FileInfo file)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(IsFileLocked);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"file: {file}"),
            Constants.Logs.LogType.DEBUG));
            bool output = false;

            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                output = true;
                return output;
            }
            finally
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                Constants.Logs.LogType.DEBUG));
            }
            _logService.WriteLog(logs);
            return output;
        }

        private bool Unlocked(string filepath)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(Unlocked);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"filepath: {filepath}"),
            Constants.Logs.LogType.DEBUG));
            bool output = false;

            try
            {
                // Attempts to open then close the file in RW mode, denying other users to place any locks.
                FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                fs.Close();
                output = true;
                return output;
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), $"filepath: {filepath}", string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                return output;
            }
            finally
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                Constants.Logs.LogType.DEBUG));
                _logService.WriteLog(logs);
            }
        }

        private bool CreateNewXmlFile(string requestFolder, string fileName, string content)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(CreateNewXmlFile);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"requestFolder: {requestFolder}, file: {fileName}, content: {content}"),
                    Constants.Logs.LogType.DEBUG));
            bool output = false;

            try
            {
                var file = Path.Combine(requestFolder, fileName);
                var xmlDocument = JsonConvert.DeserializeXmlNode(content);

                if (xmlDocument == null)
                {
                    logs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.DESERIALIZE, $"content: {content}"),
                    Constants.Logs.LogType.ERROR));
                    return output;
                }

                XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", "no");
                XmlElement? root = xmlDocument.DocumentElement;

                if (root == null)
                {
                    logs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.ROOT_ELEMENT, $"xmlDeclaration: {xmlDeclaration}"),
                    Constants.Logs.LogType.ERROR));
                    return output;
                }

                xmlDocument.InsertBefore(xmlDeclaration, root);
                File.WriteAllText(file, xmlDocument.InnerXml);
                output = true;
                return output;
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), $"requestFolder: {requestFolder}, file: {fileName}", string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                return output;
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

        private void SendXmlResponse(string responseFolder, string username, string password, IHubCallerClients clients, int timeout, CancellationToken cancellationToken)
        {
            using var watcher = new FileSystemWatcher(responseFolder);

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Created += (sender, e) => OnCreated(sender, e, username, password, clients);
            watcher.Error += (sender, e) => OnError(sender, e, clients);
            watcher.Filter = "OQSsiquc01res_*.xml";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            int elapsed = 0;
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(timeout);
            while (!flag && !cancellationToken.IsCancellationRequested && (elapsed < timeSpan.TotalSeconds))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                elapsed += 1;
            }
        }

        private void OnCreated(object sender, FileSystemEventArgs e, string username, string password, IHubCallerClients clients)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(OnCreated);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

            var responseFile = GetXmlFileInfo(e.FullPath, username, password);

            if (responseFile != null)
            {
                var response = _handler.Response(responseFile, Constants.Message.SUCCESS, 200);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogInfo(this, methodName, Constants.Message.SUCCESS, $"responseFile: {responseFile.Filename}"),
                Constants.Logs.LogType.INFO));
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                Constants.Logs.LogType.DEBUG));
                clients.Caller.SendAsync(MethodList.DetectXmlFile, response).Wait();
                StopWatcher((FileSystemWatcher)sender, username, password, clients);
                flag = true;
            }
            _logService.WriteLog(logs);
        }

        private void StopWatcher(FileSystemWatcher watcher, string username, string password, IHubCallerClients clients)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Created -= (sender, e) => OnCreated(sender, e, username, password, clients);
            watcher.Dispose();
        }

        private void SendXmlResponse(string responseFolder, string requestComparation, string username, string password, IHubCallerClients clients, int timeout, CancellationToken cancellationToken)
        {
            using var watcher = new FileSystemWatcher(responseFolder)
            {
                Filters = { "OQSsiquc01res_*.xml", "OQSsiquc01req_*.xml", "OQSmuquc01req_*.xml", "OQSmuquc02req_*.xml", "OQSmuquc01res_*.xml", "OQSmuquc02res_*.xml", "OQSmuquc01req_*.err", "OQSmuquc02req_*.err" }
            };

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Created += (sender, e) => OnCreated(sender, e, requestComparation, username, password, clients);
            watcher.Error += (sender, e) => OnError(sender, e, clients);
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            int elapsed = 0;
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(timeout);
            while (!flag && !cancellationToken.IsCancellationRequested && (elapsed < timeSpan.TotalSeconds))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                elapsed += 1;
            }
        }

        private void OnCreated(object sender, FileSystemEventArgs e, string requestComparation, string username, string password, IHubCallerClients clients)
        {
            var logs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            string methodName = nameof(OnCreated);
            var responseFile = GetXmlFileInfo(e.FullPath, username, password);
            string response;

            if (responseFile != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(e.FullPath);
                var extension = Path.GetExtension(e.FullPath);
                var listRequest = new List<string> { "OQSsiquc01req_", "OQSmuquc01req_", "OQSmuquc02req_" };
                var listResponse = new List<string> { "OQSsiquc01res_", "OQSmuquc01res_", "OQSmuquc02res_" };
                if (listResponse.Any(i => fileName.StartsWith(i)))
                {
                    var responseContent = responseFile.Content;

                    if (!string.IsNullOrEmpty(responseContent) && responseContent.Contains(requestComparation))
                    {
                        response = _handler.Response(responseFile, Constants.Message.SUCCESS, 200);
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogInfo(this, methodName, Constants.Message.SUCCESS, $"responseFile: {responseFile.Filename}"),
                        Constants.Logs.LogType.INFO));
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                        StopWatcher((FileSystemWatcher)sender, requestComparation, username, password, clients);
                        flag = true;
                    }
                }
                else if (listRequest.Any(i => fileName.StartsWith(i)))
                {
                    var responseContent = responseFile.Content;

                    if (!string.IsNullOrEmpty(responseContent) && responseContent.Contains(requestComparation))
                    {
                        var errFile = Path.Combine(Path.GetDirectoryName(e.FullPath), Path.GetFileNameWithoutExtension(e.FullPath) + ".err");
                        var errFileInfo = GetXmlFileInfo(errFile, username, password);

                        if (errFileInfo != null)
                        {
                            response = _handler.Response(errFileInfo, Constants.Message.SUCCESS, 200);
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogInfo(this, methodName, Constants.Message.SUCCESS, $"responseFile: {errFileInfo.Filename}"),
                            Constants.Logs.LogType.INFO));
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                            Constants.Logs.LogType.DEBUG));
                            clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
                            StopWatcher((FileSystemWatcher)sender, requestComparation, username, password, clients);
                            flag = true;
                        }
                    }
                }
            }
            _logService.WriteLog(logs);
        }

        private void StopWatcher(FileSystemWatcher watcher, string requestComparation, string username, string password, IHubCallerClients clients)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Created -= (sender, e) => OnCreated(sender, e, requestComparation, username, password, clients);
            watcher.Dispose();
        }

        private void OnError(object sender, ErrorEventArgs e, IHubCallerClients clients)
        {
            var response = _handler.Response(new object(), e.GetException().Message, 400);
            clients.Caller.SendAsync(MethodList.CreateXmlFile, response).Wait();
        }

        private string? GetRequestComparation(string requestFile)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(GetRequestComparation);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"requestFile: {requestFile}"),
            Constants.Logs.LogType.DEBUG));
            string? output = null;

            try
            {
                var content = File.ReadAllText(requestFile, Encoding.UTF8);

                if (string.IsNullOrEmpty(content))
                    return output;

                var firstIndex = content.IndexOf(Constants.Tags.Open.ARBITRARY_FILE_IDENTIFIER);

                if (firstIndex == -1)
                    return output;

                var lastIndex = content.LastIndexOf(Constants.Tags.Close.ARBITRARY_FILE_IDENTIFIER) + Constants.Tags.Close.ARBITRARY_FILE_IDENTIFIER.LastIndexOf('>') + 1;

                if (lastIndex == -1)
                    return output;

                output = content[firstIndex..lastIndex];
                return output;
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), $"fileName: {requestFile}", string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                return output;
            }
            finally
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                Constants.Logs.LogType.DEBUG));
                _logService.WriteLog(logs);
            }
        }
        private string? GetRequestComparationReceptionNumber(string requestFile)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(GetRequestComparationReceptionNumber);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"requestFile: {requestFile}"),
            Constants.Logs.LogType.DEBUG));
            string? output = null;

            try
            {
                var content = File.ReadAllText(requestFile, Encoding.UTF8);

                if (string.IsNullOrEmpty(content))
                    return output;

                var firstIndex = content.IndexOf(Constants.Tags.Open.RECEPTION_NUMBER);

                if (firstIndex == -1)
                    return output;

                var lastIndex = content.LastIndexOf(Constants.Tags.Close.RECEPTION_NUMBER) + Constants.Tags.Close.RECEPTION_NUMBER.LastIndexOf('>') + 1;

                if (lastIndex == -1)
                    return output;

                output = content[firstIndex..lastIndex];
                return output;
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), $"fileName: {requestFile}", string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                return output;
            }
            finally
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                Constants.Logs.LogType.DEBUG));
                _logService.WriteLog(logs);
            }
        }
        private bool IsKensaIraiCreated(List<string> outputPaths, KensaIraiFileRequest request, int sinDate, int raiinNo)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(IsKensaIraiCreated);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"paths.Count: {outputPaths.Count}, request.Output.Count: {request.Output.Count}, request.OutputDummy.Count: {request.OutputDummy.Count}, request.RaiinInfKaId: {request.RaiinInfKaId}, request.KensaInfIraiCd: {request.KensaInfIraiCd}, request.PtInfPtNum: {request.PtInfPtNum}, request.RaiinNo: {raiinNo}"),
            Constants.Logs.LogType.DEBUG));

            bool result = false;

            try
            {
                string tempOutputPath = Constants.Paths.KensaIrai.ODRKENSAIRAI_TEMP_PATH;
                string tempOutputfile = Path.Combine(tempOutputPath, $"{Constants.JAPAN_TIME:yyyyMMddHHmmssfff}.txt");

                if (!FileUtil.ExistMakeDir(Path.GetDirectoryName(tempOutputfile)))
                {
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.NOT_EXIST, $"path: {tempOutputPath}"),
                    Constants.Logs.LogType.ERROR));
                }
                else
                {
                    if (!FileUtil.SaveTextFile(Path.Combine(Constants.Paths.KensaIrai.ODRKENSAIRAI_TEMP_PATH, tempOutputfile), false, "Shift-Jis", request.Output, 5, 100))
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.CREATING, $"path: {Constants.Paths.KensaIrai.ODRKENSAIRAI_TEMP_PATH}, fileName: {tempOutputfile}"),
                        Constants.Logs.LogType.ERROR));
                    }
                }

                foreach (string outputPath in outputPaths)
                {
                    string path = ReplaceParam(outputPath, request.KensaInfIraiCd, request.PtInfPtNum, sinDate);

                    if (!FileUtil.ExistMakeDir(Path.GetDirectoryName(path)))
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.NOT_EXIST, $"path: {path}"),
                        Constants.Logs.LogType.ERROR));
                    }
                    else
                    {
                        if (!FileUtil.FileCopy(Path.Combine(Constants.Paths.KensaIrai.ODRKENSAIRAI_TEMP_PATH, tempOutputfile), path, 5, 100))
                        {
                            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.CREATING, $"path: {path}, fileName: {tempOutputfile}"),
                            Constants.Logs.LogType.ERROR));
                            ;
                        }
                    }
                }

                var outputDummyPath = Constants.Paths.KensaIrai.ODRKENSAIRAI;
                var outputDummyFile = Path.Combine(outputDummyPath, $"{Constants.JAPAN_TIME:yyyyMMdd}_{request.PtInfPtNum}_{raiinNo}.txt");

                if (!FileUtil.ExistMakeDir(Path.GetDirectoryName(outputDummyFile)))
                {
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Path.NOT_EXIST, $"path: {outputDummyPath}"),
                    Constants.Logs.LogType.ERROR));
                }
                else
                {
                    if (FileUtil.IsFileExisting(outputDummyFile))
                    {
                        File.Delete(outputDummyFile);
                    }

                    if (!FileUtil.SaveTextFile(outputDummyFile, false, "Shift-Jis", request.OutputDummy, 5, 100))
                    {
                        logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.CREATING, $"path: {outputDummyPath}, fileName: {outputDummyFile}"),
                        Constants.Logs.LogType.ERROR));
                    }
                    else
                        result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                return result;
            }
            finally
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {result}"),
                Constants.Logs.LogType.DEBUG));
                _logService.WriteLog(logs);
            }
        }

        private string ReplaceParam(string param, long iraiKey, int ptInfNum, int sinDate)
        {
            #region local method

            void Replace(string oldValue, string newValue)
            {
                param = param.Replace($"\"{oldValue}\"", newValue);
            }

            string GetFieldName(string baseStr)
            {
                int posStart = 0;
                int posEnd = 0;
                string repStr = "";
                posStart = param.IndexOf($"\"{baseStr}");
                posEnd = param.IndexOf("\"", posStart + 1);
                if (posStart >= 0 && posEnd >= 0)
                {
                    repStr = param.Substring(posStart, posEnd - posStart + 1);
                    repStr = repStr.Replace("\"", "");
                }

                return repStr;
            }

            int StrToIntDef(string str, int defaultVal)
            {
                int ret;

                if (int.TryParse(str, out ret) == false)
                {
                    ret = defaultVal;
                }

                return ret;
            }

            (string repStr, int len) GetLengthFieldData(string baseStr)
            {
                string repStr = GetFieldName(baseStr);
                int len = 0;
                if (string.IsNullOrEmpty(repStr) == false)
                {
                    len = StrToIntDef(repStr.Substring($"{baseStr}".Length, repStr.Length - $"{baseStr}".Length), 0);
                }

                return (repStr, len);
            }

            #endregion local method

            string rep;
            int length;

            Replace("patientid", $"{ptInfNum}");

            (rep, length) = GetLengthFieldData("patientid_z");
            if (string.IsNullOrEmpty(rep) == false)
            {
                if (length == 0)
                {
                    length = 9;
                }
                Replace(rep, ptInfNum.ToString().PadLeft(length, '0'));
            }

            (rep, length) = GetLengthFieldData("iraikey_z");
            Replace(rep, iraiKey.ToString().PadLeft(length, '0'));

            List<(string, string)> ReplaceTimeParamString = new()
            {
                ("sysdate", "yyyy/MM/dd HH:mm:ss"),
                ("sysdate_2", "yyyyMMddHHmmss"),
                ("sysdate_3", "yyyyMMddHHmm"),
                ("sysdate_4", "yyyyMMdd-HHmmzzz"),
                ("sysdate_hh", "HH"),
                ("sysdate_nn", "mm"),
                ("sysdate_ss", "ss"),
                ("sysdate_zzz", "fff")
            };

            DateTime dt = DateTime.Now;

            foreach ((string oldVal, string newVal) in ReplaceTimeParamString)
            {
                Replace(oldVal, dt.ToString(newVal));
            }
            Replace("sysdate_yyyy", dt.ToString("yyyy"));
            Replace("sysdate_yy", dt.ToString("yyyy").Substring(2, 2));
            Replace("sysdate_mm", dt.ToString("MM"));
            Replace("sysdate_dd", dt.ToString("dd"));
            Replace("calendate", $"{sinDate}");

            return param;
        }
    }
}