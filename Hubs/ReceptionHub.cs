﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineService.Base;
using OnlineService.Request;
using System.Threading.Tasks;

namespace OnlineService.Hubs
{
    public class ReceptionHub : BaseHub
    {
        public ReceptionHub()
        {
        }

        public async Task CreateXmlFile([FromBody] CreateXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(CreateXmlFile), $"Message: Request from {nameof(ReceptionHub)}", $"Method: {MethodList.CreateXmlFile}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.CreateXmlFile);
            if (isNewVersion) { return; }
#if DEBUG
            _xmlService.CreateXmlFile(request, _config.ListXmlPathEndpointDev, string.Empty, string.Empty, Clients, _config.Timeout, Context.ConnectionAborted);
#else
            _xmlService.CreateXmlFile(request, _config.ListXmlPathEndpointStg, string.Empty, string.Empty, Clients, _config.Timeout, Context.ConnectionAborted);
#endif
        }

        public async Task CreateXmlFileUpdateRefNo([FromBody] CreateXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(CreateXmlFileUpdateRefNo), $"Message: Request from {nameof(ReceptionHub)}", $"Method: {MethodList.CreateXmlFileUpdateRefNo}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.CreateXmlFileUpdateRefNo);
            if (isNewVersion) { return; }
#if DEBUG
            string result = _xmlService.CreateXmlFileUpdateRefNo(request, _config.ListXmlPathEndpointDev);
#else
            string result = _xmlService.CreateXmlFileUpdateRefNo(request, _config.ListXmlPathEndpointStg);
#endif
            await Clients.Caller.SendAsync(MethodList.CreateXmlFileUpdateRefNo, result);
        }

        public async Task MoveXmlFile([FromBody] MoveXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(MoveXmlFile), $"Request from {nameof(ReceptionHub)}", $"Method: {MethodList.MoveXmlFile}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.MoveXmlFile);
            if (isNewVersion) { return; }
            string result = _xmlService.MoveXmlFile(request);
            await Clients.Caller.SendAsync(MethodList.MoveXmlFile, result);
        }

        public override string GetKey()
        {
            return this.GetType().Name;
        }
    }
}