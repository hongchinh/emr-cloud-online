using log4net.Appender;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using OnlineService.Base;
using OnlineService.Request;
using OnlineService.Response;
using System.Diagnostics;
using Newtonsoft.Json;
using OnlineService.Util;
using System.Net.Http;
using System.Net;
using System.Xml;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig;

namespace OnlineService.Services
{
    public class FileService
    {
        private readonly Handler _handler = new Handler();
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        private bool flag;
        private CancellationToken token;
        protected ApiService _logService = new ApiService();

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
        public void CreateFile(CreateXmlRequest request, string endpoint, string username, string password, IHubCallerClients clients, int timeout, CancellationToken cancellationToken)
        {
            string methodName = nameof(CreateFile);
            string? response = null;
            string? responseFolder = string.Empty;
            var logs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

            if (request == null)
            {
                logs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.REQUEST_NULL, $"request: {nameof(CreateXmlRequest)}"),
                    Constants.Logs.LogType.ERROR));
                response = _handler.Response(new object(), Constants.Message.Web.Error.File.REQUEST_NULL, 400);
                clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
                _logService.WriteLog(logs);
                return;
            }
            logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"request.ScreenCode: {request.ScreenCode}, request.Domain: {request.Domain}, request.Token: {request.Token}, request.FileName: {request.FileName}, request.Content: {request.Content}, endpoint: {endpoint}"),
                Constants.Logs.LogType.DEBUG));
            string pathRequest = string.Empty;
            switch (request.ScreenCode)
            {
                case Constants.ScreenCode.MEDICAL:
                    string message;
                    var requestFolders = GetListXmlPath(Constants.PathConf.GRPCD_101, endpoint, request.Domain, request.Token, out message, out List<WriteLogRequest> writeLogs);
                    logs.AddRange(writeLogs);
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
                            response = _handler.Response(new object(), Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT, 400);
                            clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
                            _logService.WriteLog(logs);
                            return;
                        }

                        if (message.Contains(Constants.Message.Web.Error.Api.INVALID_TOKEN))
                        {
                            #region [Write logs]
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                            Constants.Logs.LogType.DEBUG));
                            logs.Add(new WriteLogRequest(
                            Constants.Logs.EventCd.SIGNALR,
                            Log.WriteLogError(this, methodName, message),
                            Constants.Logs.LogType.ERROR));
                            #endregion
                            response = _handler.Response(new object(), message, 401);
                            clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
                            _logService.WriteLog(logs);
                            return;
                        }
                    }

                    var responseFolders = GetListXmlPath(Constants.PathConf.GRPCD_102, endpoint, request.Domain, request.Token, out message, out List<WriteLogRequest> writeLog);
                    logs.AddRange(writeLogs);
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
                            response = _handler.Response(new object(), Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT, 400);
                            clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
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
                            clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
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
                        clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
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
                        clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
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
                        clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
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
                        clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
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
                        clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }

                    if (!request.FileName.StartsWith("TKKsiquc01req_") && !request.FileName.StartsWith("YZKsiquc01req_") || !Path.GetExtension(request.FileName).Equals(".xml"))
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
                        clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }
                    token = cancellationToken;
                    if (token.IsCancellationRequested)
                    {
                        FileUtil.DeleteFile(pathRequest);
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        response = _handler.Response(new object(), "The process has been aborted", 400);
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
                        clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
                        _logService.WriteLog(logs);
                        return;
                    }

                    var requestFile = Path.Combine(requestFolder, request.FileName);
                    pathRequest = requestFile;
                    var requestComparation = GetRequestComparation(requestFile);

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
                        clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
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
                    clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
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
                    clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
                    _logService.WriteLog(logs);
                    return;
                }
            }
            _logService.WriteLog(logs);
        }

        public string MoveFile(MoveXmlRequest request)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(MoveFile);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

            if (request == null)
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.REQUEST_NULL, $"request: {nameof(MoveXmlRequest)}"),
                Constants.Logs.LogType.ERROR));
                _logService.WriteLog(logs);
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
                _logService.WriteLog(logs);
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
                _logService.WriteLog(logs);
                return _handler.Response(new object(), Constants.Message.Web.Error.File.DUPLICATE_FILE, 400);
            }

            switch (request.ScreenCode)
            {
                case Constants.ScreenCode.MEDICAL:
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
                                _logService.WriteLog(logs);
                                return _handler.Response(new object(), Constants.Message.Web.Error.File.MOVING, 400);
                            }
                        }
                    }

                    output = _handler.Response(new object(), Constants.Message.SUCCESS, 200);
                    Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}");
                    Log.WriteLogInfo(this, methodName, Constants.Message.SUCCESS, $"Result: {request.Files} file(s)");
                    _logService.WriteLog(logs);
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
            _logService.WriteLog(logs);
            return _handler.Response(new object(), Constants.Message.Web.Error.ScreenCode.INVALID, 400);
        }

        public string ConvertXmlFileToJson(string xmlFilePath)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(ConvertXmlFileToJson);
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                string jsonText = JsonConvert.SerializeXmlNode(xmlDoc);
                return jsonText;
            }
            catch (Exception ex)
            {
                Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.APP,
                Log.WriteLogException(this, methodName, $"Exeption: " + ex.Message, ""),
                Constants.Logs.LogType.EXCEPTION));
                _logService.WriteLog(logs);
                return ex.Message;
            }
        }

        private FileData? GetFileData(string pathFile, string username, string password)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(GetFileData);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"xmlFile: {pathFile}"),
            Constants.Logs.LogType.DEBUG));
            FileData? output = null;

            try
            {
                if (!File.Exists(pathFile))
                {
                    logs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.SIGNALR,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.File.NOT_EXIST, $"file: {pathFile}"),
                    Constants.Logs.LogType.ERROR));
                    return output;
                }

                var fileInfo = new FileInfo(pathFile);

                if (IsFileLocked(fileInfo))
                {
                    var isUnlocked = Unlocked(pathFile);
                    if (!isUnlocked)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }

                if (IsFileLocked(fileInfo))
                {
                    List<Process> lstProcs = new List<Process>();
                    lstProcs = ProcessHandler.WhoIsLocking(pathFile);

                    foreach (Process p in lstProcs)
                    {
                        if (p.MachineName == ".")
                            ProcessHandler.LocalProcessKill(p.ProcessName);
                        else
                            ProcessHandler.RemoteProcessKill(p.MachineName, username, password, p.ProcessName);
                    }
                }

                string content = string.Empty;
                string contentFile = string.Empty;
                string contentBase64 = string.Empty;
                Enum.TypeFileEnum typeFile = 0;
                var extenstion = Path.GetExtension(pathFile);
                if (extenstion.Contains("xml"))
                {
                    using (var fs = new FileStream(pathFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        contentFile = sr.ReadToEnd();
                    }
                    content = ConvertXmlFileToJson(pathFile);
                    typeFile = Enum.TypeFileEnum.XML;
                }

                else if (extenstion.Contains("pdf"))
                {
                    Byte[] bytes = File.ReadAllBytes(pathFile);
                    contentBase64 = Convert.ToBase64String(bytes);
                    typeFile = Enum.TypeFileEnum.PDF;
                    using (var pdf = PdfDocument.Open(pathFile))
                    {
                        foreach (var page in pdf.GetPages())
                        {
                            var text = ContentOrderTextExtractor.GetText(page);
                            var rawText = page.Text;
                            contentFile += rawText;
                        }
                    }
                }
                output = new FileData
                {
                    Content = content,
                    ContentFile = contentFile,
                    ContentBase64 = contentBase64,
                    Filename = pathFile,
                    TypeFile = typeFile
                };
                return output;
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
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

        private List<string> GetListXmlPath(int type, string endpoint, string domain, string token, out string message, out List<WriteLogRequest> writeLogs, bool isKensaIrai = false)
        {
            string methodName = nameof(GetListXmlPath);
            message = string.Empty;
            writeLogs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.API);
            List<string>? result = null;

            try
            {
                var machine = Environment.MachineName;
                endpoint = string.Format(endpoint, type, machine, isKensaIrai);
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", token);
                client.DefaultRequestHeaders.Add("Domain", domain);
                HttpResponseMessage response = client.GetAsync(endpoint).Result;

                writeLogs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.API,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"type: {type}, endpoint: {endpoint}, domain: {domain}, token: {token}, isKensaIrai: {isKensaIrai}"),
                Constants.Logs.LogType.DEBUG));
                writeLogs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.API,
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
                    writeLogs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.API,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.SignalR.Api.INVALID_TOKEN, $"token: {token}"),
                    Constants.Logs.LogType.ERROR));
                    return new List<string>();
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                writeLogs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.API,
                    Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), $"type: {type}, endpoint: {endpoint}, domain: {domain}, token: {token}, message: {message}, isKensaIrai: {isKensaIrai}", string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                    Constants.Logs.LogType.ERROR));
                message = ex.Message;
                return new List<string>();
            }
            finally
            {
                if (result?.Count > 0)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        writeLogs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.API,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Path {i}: {result[i]}"),
                        Constants.Logs.LogType.DEBUG));
                    }
                }
                else
                {
                    writeLogs.Add(new WriteLogRequest(
                    Constants.Logs.EventCd.API,
                    Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {result}"),
                    Constants.Logs.LogType.DEBUG));
                }
            }
        }

        private bool IsFileLocked(FileInfo file)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(IsFileLocked);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
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
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                output = true;
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
            return output;
        }

        private bool Unlocked(string filepath)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(Unlocked);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
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
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), $"filepath: {filepath}", string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
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

        private void SendXmlResponse(string responseFolder, string requestComparation, string username, string password, IHubCallerClients clients, int timeout, CancellationToken cancellationToken)
        {
            using var watcher = new FileSystemWatcher(responseFolder)
            {
                Filters = { "TKKsiquc01req_*.xml", "TKKsiquc01res_*.xml", "YZKsiquc01req_*.xml", "YZKsiquc01res_*.xml", "TKKsiquc01res_*.pdf", "YZKsiquc01res_*.pdf" }
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
            var responseFile = GetFileData(e.FullPath, username, password);
            string response;

            if (responseFile != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(e.FullPath);
                var extension = Path.GetExtension(e.FullPath);

                if (fileName.StartsWith("TKKsiquc01res_") || fileName.StartsWith("YZKsiquc01res_"))
                {
                    var responseContent = responseFile.ContentFile;

                    if (!string.IsNullOrEmpty(responseContent) && responseContent.Contains(requestComparation))
                    {
                        response = _handler.Response(responseFile, Constants.Message.SUCCESS, 200);
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogInfo(this, methodName, Constants.Message.SUCCESS, $"responseFile: {responseFile.Filename}"),
                        Constants.Logs.LogType.INFO));
                        logs.Add(new WriteLogRequest(
                        Constants.Logs.EventCd.SIGNALR,
                        Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {response}"),
                        Constants.Logs.LogType.DEBUG));
                        clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
                        StopWatcher((FileSystemWatcher)sender, requestComparation, username, password, clients);
                        flag = true;
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
            clients.Caller.SendAsync(MethodList.CreateFile, response).Wait();
        }

        private string? GetRequestComparation(string requestFile)
        {
            var logs = new List<WriteLogRequest>();
            string methodName = nameof(GetRequestComparation);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            logs.Add(new WriteLogRequest(
            Constants.Logs.EventCd.SIGNALR,
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
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), $"fileName: {requestFile}", string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
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
    }
}