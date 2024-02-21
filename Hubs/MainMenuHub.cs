using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineService.Base;
using OnlineService.Request;
using System.Threading.Tasks;

namespace OnlineService.Hubs
{
    public class MainMenuHub : BaseHub
    {
        public MainMenuHub() { }

        public async Task CreateXmlFile([FromBody] CreateXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(CreateXmlFile), $"Message: Request from {nameof(MainMenuHub)}", $"Method: {MethodList.CreateXmlFile}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.CreateXmlFile);
            if (isNewVersion) { return; }
#if DEBUG
            _xmlService.CreateXmlFile(request, _config.ListXmlPathEndpointDev, _config.Username, _config.Password, Clients, _config.Timeout, Context.ConnectionAborted);
#else
            _xmlService.CreateXmlFile(request, _config.ListXmlPathEndpointStg, _config.Username, _config.Password, Clients, _config.Timeout, Context.ConnectionAborted);
#endif
        }

        public async Task MoveXmlFile([FromBody] MoveXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(MoveXmlFile), $"Request from {nameof(MainMenuHub)}", $"Method: {MethodList.MoveXmlFile}");
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
