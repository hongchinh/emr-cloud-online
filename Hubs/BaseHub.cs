using log4net.Appender;
using log4net;
using Microsoft.AspNetCore.SignalR;
using OnlineService.Base;
using OnlineService.Services;
using System;
using System.Threading.Tasks;

namespace OnlineService.Hubs
{
    public class BaseHub : Hub
    {
        public static readonly ConnectionMapping<string> _connections = new ConnectionMapping<string>();
        protected JsonConfig _config = new JsonConfig();
        protected XmlFileService _xmlService = new XmlFileService();
        protected FileService _fileService = new FileService();
        protected UpdateAppService _updateAppService = new UpdateAppService();
        protected FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];

        public IClientProxy GetClient
        {
            get
            {
                return Clients.Clients(_connections.GetConnections(GetKey()));
            }
        }

        public BaseHub()
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
        }

        public override async Task OnConnectedAsync()
        {
            _connections.Add(GetKey(), Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            _connections.Remove(GetKey(), Context.ConnectionId);

            await base.OnDisconnectedAsync(ex);
        }

        public virtual string GetKey()
        {
            throw new NotImplementedException("Please implement me!!!");
        }
    }
}