using log4net;
using log4net.Appender;
using Newtonsoft.Json;
using OnlineService.Request;
using OnlineService.Response;
using OnlineService.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace OnlineService.Services
{
    public class ApiService
    {
        protected JsonConfig _config = new JsonConfig();
        private readonly Handler _handler = new Handler();
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        public ApiService() { }
        public void WriteLog(List<WriteLogRequest> request)
        {
            Task.Run(() =>
            {
                string methodName = nameof(WriteLog);
                string output = string.Empty;
                var ipv4 = GetIpV4();
                var settingFile = AppDomain.CurrentDomain.BaseDirectory + "setting.ini";
                try
                {
                    if (request.Count == 0)
                    {
                        return;
                    }
                    var token = string.Empty;
                    if (File.Exists(settingFile))
                    {
                        var iniFile = new IniFile(settingFile);
                        token = iniFile.GetValue("Online", "Token");
                    }
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    client.DefaultRequestHeaders.Add("Domain", "SmartKarteApp");
                    string machine = GetMachineName();
                    request.ForEach(i => i.Description = machine + "-" + ipv4);
                    var jsonString = new
                    {
                        writeListLogRequests = request,
                    };
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(jsonString), Encoding.UTF8, "application/json");
                    if (!NetworkInterface.GetIsNetworkAvailable() || !NetworkUtil.IsInternetConnected())
                    {
                        Log.WriteLogInfo(this, methodName, Constants.Message.Log.Error.App.CheckUpdate.LOST_CONNECTION);
                        return;
                    }
#if DEBUG
                    HttpResponseMessage response = client.PostAsync(_config.WriteLogEndpointDev, content).Result;
#else
                    HttpResponseMessage response = client.PostAsync(_config.WriteLogEndpointStg, content).Result;
#endif
                    Log.SwitchFileAppender(_appender, Constants.Logs.API);
                    Log.WriteLogInfo(this, methodName, $"Request from Machine: {machine}", $"Status Code: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        output = json.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLogException(this, methodName, $"Exception: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                }
                finally
                {
                    Log.WriteLogDebug(this, methodName, "Output", $" Result: {output}");
                }
            });
        }

        public bool UpdateSignalRPort(int portNumber)
        {
            string methodName = nameof(UpdateSignalRPort);
            string output = string.Empty;
            var ipv4 = GetIpV4();
            var settingFile = AppDomain.CurrentDomain.BaseDirectory + "setting.ini";
            try
            {
                var token = string.Empty;
                if (File.Exists(settingFile))
                {
                    var iniFile = new IniFile(settingFile);
                    token = iniFile.GetValue("Online", "Token");
                }
                var machine = GetMachineName();
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("Domain", "SmartKarteApp");
                var body = new UpdateSignalRPortRequest();
                body.Ip = ipv4;
                body.PortNumber = portNumber;
                body.MachineName = machine;
                HttpContent content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                if (!NetworkInterface.GetIsNetworkAvailable() || !NetworkUtil.IsInternetConnected())
                {
                    Log.WriteLogInfo(this, methodName, Constants.Message.Log.Error.App.CheckUpdate.LOST_CONNECTION);
                    return false;
                }
#if DEBUG
                HttpResponseMessage response = client.PostAsync(_config.UpdateSignalRPortEndpointDev, content).Result;
#else
                HttpResponseMessage response = client.PostAsync(_config.UpdateSignalRPortEndpointStag, content).Result;
#endif
                Log.SwitchFileAppender(_appender, Constants.Logs.API);
                Log.WriteLogInfo(this, methodName, $"Request from Machine: {machine}", $"Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    output = json.ToString();
                    var myDeserializedClass = JsonConvert.DeserializeObject<UpdateSignalRPortResponse>(json);
                    if (myDeserializedClass != null)
                    {
                        if (myDeserializedClass.Status == 1)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.WriteLogException(this, methodName, $"Exception: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                Log.WriteLogDebug(this, methodName, "Output", $" Result: {output}");
            }
        }

        public int GetSignalRPort()
        {
            string methodName = nameof(GetSignalRPort);
            string output = string.Empty;
            var ipv4 = GetIpV4();
            var settingFile = AppDomain.CurrentDomain.BaseDirectory + "setting.ini";
            try
            {
                var token = string.Empty;
                if (File.Exists(settingFile))
                {
                    var iniFile = new IniFile(settingFile);
                    token = iniFile.GetValue("Online", "Token");
                }
                var machine = GetMachineName();
#if DEBUG
                var endpoint = string.Format(_config.GetSignalRPortEndpointDev, machine, ipv4);
#else
                var endpoint = string.Format(_config.GetSignalRPortEndpointStag, machine, ipv4);
#endif
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("Domain", "SmartKarteApp");
                if (!NetworkInterface.GetIsNetworkAvailable() || !NetworkUtil.IsInternetConnected())
                {
                    Log.WriteLogInfo(this, methodName, Constants.Message.Log.Error.App.CheckUpdate.LOST_CONNECTION);
                    return 0;
                }
                HttpResponseMessage response = client.GetAsync(endpoint).Result;
                Log.SwitchFileAppender(_appender, Constants.Logs.API);
                Log.WriteLogInfo(this, methodName, $"Request from Machine: {machine}", $"Status Code: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    output = json.ToString();
                    var myDeserializedClass = JsonConvert.DeserializeObject<GetSignalRPortResponse>(json);
                    if (myDeserializedClass != null)
                    {
                        if (myDeserializedClass.Status == 1)
                        {
                            return myDeserializedClass.Data.SmartKarteAppSignalRPort.PortNumber;
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Log.WriteLogException(this, methodName, $"Exception: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                return 0;
            }
            finally
            {
                Log.WriteLogDebug(this, methodName, "Output", $" Result: {output}");
            }
        }
        private string GetIpV4()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            var ipv4 = string.Empty;
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipv4 = ip.ToString();
                    break;
                }
            }
            return ipv4;
        }
        private string GetMachineName()
        {
            return Environment.MachineName;
        }
    }
}
