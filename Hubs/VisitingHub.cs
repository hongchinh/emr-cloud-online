using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineService.Base;
using OnlineService.Request;
using System.Threading.Tasks;

namespace OnlineService.Hubs
{
    public class VisitingHub : BaseHub
    {
        public VisitingHub()
        {
        }

        public async Task GetListXmlFile([FromBody] GetXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(GetListXmlFile), $"Message: Request from {nameof(VisitingHub)}", $"Method: {MethodList.GetListXmlFile}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.GetListXmlFile);
            if (isNewVersion) { return; }
#if DEBUG
            string result = _xmlService.GetListXmlFile(request, _config.ListXmlPathEndpointDev);
#else
            string result = _xmlService.GetListXmlFile(request, _config.ListXmlPathEndpointStg);
#endif
            await Clients.Caller.SendAsync(MethodList.GetListXmlFile, result);
        }

        public async Task DetectXmlFile([FromBody] GetXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(DetectXmlFile), $"Request from {nameof(VisitingHub)}", $"Method: {MethodList.DetectXmlFile}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.DetectXmlFile);
            if (isNewVersion) { return; }
#if DEBUG
            _xmlService.DetectXmlFile(request, _config.ListXmlPathEndpointDev, _config.Timeout, string.Empty, string.Empty, Clients, Context.ConnectionAborted);
#else
            _xmlService.DetectXmlFile(request, _config.ListXmlPathEndpointStg, _config.Timeout, string.Empty, string.Empty, Clients, Context.ConnectionAborted);
#endif
        }

        public async Task MoveXmlFile([FromBody] MoveXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(MoveXmlFile), $"Request from {nameof(VisitingHub)}", $"Method: {MethodList.MoveXmlFile}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.MoveXmlFile);
            if (isNewVersion) { return; }
            string result = _xmlService.MoveXmlFile(request);
            await Clients.Caller.SendAsync(MethodList.MoveXmlFile, result);
        }

        public async Task CreateXmlFileUpdateRefNo([FromBody] CreateXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(CreateXmlFileUpdateRefNo), $"Request from {nameof(VisitingHub)}", $"Method: {MethodList.CreateXmlFileUpdateRefNo}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.CreateXmlFileUpdateRefNo);
            if (isNewVersion) { return; }
#if DEBUG
            string result = _xmlService.CreateXmlFileUpdateRefNo(request, _config.ListXmlPathEndpointDev);
#else
            string result = _xmlService.CreateXmlFileUpdateRefNo(request, _config.ListXmlPathEndpointStg);
#endif
            await Clients.Caller.SendAsync(MethodList.CreateXmlFileUpdateRefNo, result);
        }

        public override string GetKey()
        {
            return this.GetType().Name;
        }
    }
}