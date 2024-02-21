using OnlineService.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace OnlineService.Services
{
    public class SignalRPortService
    {
        private readonly ApiService _logService = new ApiService();
        public bool IsFreePort(int port)
        {
            var logs = new List<WriteLogRequest>();
            var methodName = nameof(IsFreePort);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"port: {port}"),
            Constants.Logs.LogType.DEBUG));
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] listeners = properties.GetActiveTcpListeners();
            int[] openPorts = listeners.Select(item => item.Port).ToArray<int>();
            var output = openPorts.All(openPort => openPort != port);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
            Constants.Logs.LogType.DEBUG));
            _logService.WriteLog(logs);
            return output;
        }
        public bool RewritePortNumber(int port)
        {
            var logs = new List<WriteLogRequest>();
            var methodName = nameof(RewritePortNumber);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"port: {port}"),
            Constants.Logs.LogType.DEBUG));
            bool output = false;

            try
            {
                string configFilePath = AppDomain.CurrentDomain.BaseDirectory + "config.json";
                var input = File.ReadAllText(configFilePath);

                if (string.IsNullOrEmpty(input))
                {
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.Json.EMPTY, $"configFilePath: {configFilePath}"),
                    Constants.Logs.LogType.ERROR));
                    return output;
                }

                dynamic configInfo = JsonConvert.DeserializeObject(input) ?? new object();
                configInfo["port"] = port;
                string result = JsonConvert.SerializeObject(configInfo, Formatting.Indented);
                File.WriteAllText(configFilePath, result);
                output = true;
                return output;
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), $"port: {port}", string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                return output;
            }
            finally
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {output}"),
                Constants.Logs.LogType.DEBUG));
                _logService.WriteLog(logs);
            }
        }

        public int FreePort(int port = 0)
        {
            var logs = new List<WriteLogRequest>();
            var methodName = nameof(FreePort);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.INPUT, $"port: {port}"),
            Constants.Logs.LogType.DEBUG));
            port = (port > 0) ? port : new Random().Next(5001, 65535);
            while (!IsFreePort(port))
            {
                port += 1;
            }
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogDebug(this, methodName, Constants.Message.Log.Debug.OUTPUT, $"Result: {port}"),
            Constants.Logs.LogType.DEBUG));
            _logService.WriteLog(logs);
            return port;
        }
    }
}
