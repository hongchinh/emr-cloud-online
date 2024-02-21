using Microsoft.AspNetCore.SignalR;
using OnlineService.Base;
using OnlineService.Services;
using System.Threading.Tasks;

namespace OnlineService.Hubs
{
    public class CommonHub : BaseHub
    {
        private VersionService _versionService = new VersionService();

        public CommonHub()
        {
        }

        public async Task GetAppVersion()
        {           
            Log.WriteLogInfo(this, nameof(GetAppVersion), $"Request from {nameof(CommonHub)}", $"Method: {MethodList.GetAppVersion}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.GetAppVersion);
            if (isNewVersion) { return; }
            string result = _versionService.GetAppVersion();
            await Clients.Caller.SendAsync(MethodList.GetAppVersion, result);
        }

        public override string GetKey()
        {
            return this.GetType().Name;
        }
    }
}