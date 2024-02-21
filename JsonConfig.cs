using Newtonsoft.Json;
using System;
using System.IO;

namespace OnlineService
{
    public class JsonConfig
    {
        private readonly ConfigInfo _configInfo;

        public string Host => _configInfo.Host;

        public int Port => _configInfo.Port;

        public string ListXmlPathEndpointDev => _configInfo.ListXmlPathEndpointDev;

        public string AuthEndpointDev => _configInfo.AuthEndpointDev;

        public string LoginEndpointDev => _configInfo.LoginEndpointDev;
        public string WriteLogEndpointDev => _configInfo.WriteLogEndpointDev;
        public string UpdateSignalRPortEndpointDev => _configInfo.UpdateSignalRPortEndpointDev;
        public string GetSignalRPortEndpointDev => _configInfo.GetSignalRPortEndpointDev;

        public string ListXmlPathEndpointStg => _configInfo.ListXmlPathEndpointStg;

        public string AuthEndpointStg => _configInfo.AuthEndpointStg;

        public string LoginEndpointStg => _configInfo.LoginEndpointStg;
        public string WriteLogEndpointStg => _configInfo.WriteLogEndpointStg;
        public string UpdateSignalRPortEndpointStag => _configInfo.UpdateSignalRPortEndpointStag;
        public string GetSignalRPortEndpointStag => _configInfo.GetSignalRPortEndpointStag;

        public int Timeout => _configInfo.Timeout;

        public int BufferSize => _configInfo.BufferSize;

        public string Encoding => _configInfo.Encoding;

        public string Username => _configInfo.Username;

        public string Password => _configInfo.Password;
        public string PathAppCloud => _configInfo.PathAppCloud;
        public string PathVersionCloud => _configInfo.PathVersionCloud;

        public JsonConfig()
        {
            string configFilePath = AppDomain.CurrentDomain.BaseDirectory + "config.json";
            var json = File.ReadAllText(configFilePath);
            _configInfo = JsonConvert.DeserializeObject<ConfigInfo>(json) ?? new ConfigInfo();
        }
    }

    public class ConfigInfo
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string ListXmlPathEndpointDev { get; set; } = string.Empty;

        public string AuthEndpointDev { get; set; } = string.Empty;

        public string LoginEndpointDev { get; set; } = string.Empty;
        public string WriteLogEndpointDev { get; set; } = string.Empty;
        public string UpdateSignalRPortEndpointDev { get; set; } = string.Empty;
        public string GetSignalRPortEndpointDev { get; set; } = string.Empty;

        public string ListXmlPathEndpointStg { get; set; } = string.Empty;

        public string AuthEndpointStg { get; set; } = string.Empty;

        public string LoginEndpointStg { get; set; } = string.Empty;
        public string WriteLogEndpointStg { get; set; } = string.Empty;
        public string UpdateSignalRPortEndpointStag { get; set; } = string.Empty;
        public string GetSignalRPortEndpointStag { get; set; } = string.Empty;

        public int Timeout { get; set; }

        public int BufferSize { get; set; }

        public string Encoding { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string PathAppCloud { get; set; } = string.Empty;
        public string PathVersionCloud { get; set; } = string.Empty;
    }
}