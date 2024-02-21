using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineService.Base;
using OnlineService.Request;
using System.Threading.Tasks;

namespace OnlineService.Hubs
{
    public class MedicalHub : BaseHub
    {
        public MedicalHub()
        {
        }

        public async Task CreateKensaIraiFile([FromBody] CreateKensaIraiFileRequest request)
        {
            Log.WriteLogInfo(this, nameof(CreateKensaIraiFile), $"Request from {nameof(MedicalHub)}", $"Method: {MethodList.CreateKensaIraiFile}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.CreateKensaIraiFile);
            if (isNewVersion) { return; }
#if DEBUG
            string result = _xmlService.CreateKensaIraiFile(request, _config.ListXmlPathEndpointDev);
#else
            string result = _xmlService.CreateKensaIraiFile(request, _config.ListXmlPathEndpointStg);
#endif
            await Clients.Caller.SendAsync(MethodList.CreateKensaIraiFile, result);
        }
        public async Task CreateFile([FromBody] CreateXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(CreateFile), $"Message: Request from {nameof(MedicalHub)}", $"Method: {MethodList.CreateFile}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.CreateFile);
            if (isNewVersion) { return; }
#if DEBUG
            _fileService.CreateFile(request, _config.ListXmlPathEndpointDev, string.Empty, string.Empty, Clients, _config.Timeout, Context.ConnectionAborted);
#else
            _fileService.CreateFile(request, _config.ListXmlPathEndpointStg, string.Empty, string.Empty, Clients, _config.Timeout, Context.ConnectionAborted);
#endif
        }
        public async Task MoveFile([FromBody] MoveXmlRequest request)
        {
            Log.WriteLogInfo(this, nameof(MoveFile), $"Request from {nameof(PatientInfoHub)}", $"Method: {MethodList.MoveFile}");
            var isNewVersion = await _updateAppService.IsCheckNewVersionFromWeb(Clients, MethodList.MoveFile);
            if (isNewVersion) { return; }
            string result = _fileService.MoveFile(request);
            await Clients.Caller.SendAsync(MethodList.MoveFile, result);
        }

        public override string GetKey()
        {
            return this.GetType().Name;
        }
    }
}