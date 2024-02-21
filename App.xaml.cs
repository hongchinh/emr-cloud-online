using log4net;
using log4net.Appender;
using OnlineService.Util;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows;

namespace OnlineService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private JsonConfig _config = new();
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        Mutex mutex;
        public App()
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            Log.WriteAppStart();

            Log.SwitchFileAppender(_appender, Constants.Logs.API);
            Log.WriteAPIStart();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            mutex = new Mutex(true, "SmartKarteAppMutex", out createdNew);
            if (createdNew)
            {
                var settingFile = AppDomain.CurrentDomain.BaseDirectory + "setting.ini";

                if (!File.Exists(settingFile))
                {
                    StartupUri = new Uri(@"\UI\Views\LoginWindow\LoginWindow.xaml", UriKind.Relative);
                }
                else
                {
                    var iniFile = new IniFile(settingFile);
                    var token = iniFile.GetValue("Online", "Token");

                    if (Authentication(token))
                        StartupUri = new Uri(@"MainWindow.xaml", UriKind.Relative);
                    else
                        StartupUri = new Uri(@"\UI\Views\LoginWindow\LoginWindow.xaml", UriKind.Relative);
                }
                base.OnStartup(e);
            }
            else
            {
                Current.Shutdown();
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            if (mutex != null)
            {
                mutex.Close();
            }
            base.OnExit(e);
        }
        private bool Authentication(string token)
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.API);
            string methodName = nameof(Authentication);
            bool output = false;

            try
            {
                var machine = Environment.MachineName;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

#if DEBUG
                HttpResponseMessage response = client.GetAsync(_config.AuthEndpointDev).Result;
#else
                HttpResponseMessage response = client.GetAsync(_config.AuthEndpointStg).Result;
#endif

                Log.WriteLogDebug(this, methodName, "Input", $" token: {token}");
                Log.WriteLogInfo(this, methodName, $"Request from Machine: {machine}", $"Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                    output = true;
                else
                    output = false;

                return output;
            }
            catch (Exception ex)
            {
                Log.WriteLogException(this, methodName, $"Exception: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                return output;
            }
            finally
            {
                Log.WriteLogDebug(this, methodName, "Output", $" Result: {output}");
            }
        }
    }
}