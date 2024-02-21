using log4net;
using log4net.Appender;
using Newtonsoft.Json;
using OnlineService.Request;
using OnlineService.Services;
using OnlineService.UI.Views.CustomMessageBoxWindow;
using OnlineService.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OnlineService.UI.Views.LoginWindow
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private JsonConfig _config = new();
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        private readonly ApiService _logService = new ApiService();
        private readonly UpdateAppService _updateAppService = new UpdateAppService();
        private bool isClosedByUser = false;

        public LoginWindow()
        {
            InitializeComponent();
            Closing += LoginWindow_Closing;
        }

        private void LoginWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isClosedByUser)
            {
                Application.Current.Shutdown();
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            isClosedByUser = true;
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var logs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            string methodName = nameof(btnLogin_Click);
            var userId = txtUser.Text;
            var password = txtPassword.Password;
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogDebug(this, methodName, "Input", $"userId: {userId}"),
            Constants.Logs.LogType.DEBUG));

            if (string.IsNullOrEmpty(userId))
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, methodName, Constants.Message.App.Error.LoginWindow.LoginButton.EMPTY_USER_ID),
                Constants.Logs.LogType.ERROR));
                MessageBox.Show(Constants.Message.App.Error.LoginWindow.EMPTY_USER_ID, "", MessageBoxButton.OK, MessageBoxImage.Error);
                _logService.WriteLog(logs);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, methodName, Constants.Message.App.Error.LoginWindow.LoginButton.EMPTY_PW),
                Constants.Logs.LogType.ERROR));
                MessageBox.Show(Constants.Message.App.Error.LoginWindow.EMPTY_PW, "", MessageBoxButton.OK, MessageBoxImage.Error);
                _logService.WriteLog(logs);
                return;
            }
            string token = string.Empty;
            var messageUnableToConnect = string.Empty;
            if (NetworkUtil.IsInternetConnected())
            {
                token = Login(userId, password, out string message);
                messageUnableToConnect = message;
            }
            else
            {
                NetworkUtil.ShowMessageBoxNetwork();
                return;
            }           
            if (messageUnableToConnect.Contains(Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT_ENG))
            {
                Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, methodName, messageUnableToConnect),
                Constants.Logs.LogType.ERROR));
                MessageBox.Show(Constants.Message.Web.Error.Api.UNABLE_TO_CONNECT, "", MessageBoxButton.OK, MessageBoxImage.Error);
                _logService.WriteLog(logs);
                return;
            }
            if (string.IsNullOrEmpty(token))
            {
                Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, methodName, Constants.Message.App.Error.LoginWindow.LoginButton.INVALID_CREDENTIALS),
                Constants.Logs.LogType.ERROR));
                MessageBox.Show(Constants.Message.App.Error.LoginWindow.INVALID_CREDENTIALS, "", MessageBoxButton.OK, MessageBoxImage.Error);
                _logService.WriteLog(logs);
                return;
            }
            var checkSaveFile = SaveIniFile(token, out List<WriteLogRequest> writeLogs);
            logs.AddRange(writeLogs);
            if (!checkSaveFile)
            {
                Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, methodName, Constants.Message.App.Error.LoginWindow.LoginButton.ERROR_SAVE_INI),
                Constants.Logs.LogType.ERROR));
                MessageBox.Show(Constants.Message.App.Error.LoginWindow.ERROR_LOGIN, "", MessageBoxButton.OK, MessageBoxImage.Error);
                _logService.WriteLog(logs);
                return;
            }
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (mainWindow == null)
            {
                mainWindow = new MainWindow();
            }
            if (!mainWindow.IsWebApp())
            {
                mainWindow?.StartSignalRServer();
            }
            if (!mainWindow.IsAutoRunTimerInitialized())
            {
                mainWindow?.InitializeAutoRunTimer();
            }
            Hide();
            mainWindow?.ShowNotifyIcon();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogInfo(this, methodName, Constants.Message.App.Error.LoginWindow.LOGIN_SUCCESS),
            Constants.Logs.LogType.INFO));
            _logService.WriteLog(logs);
            CheckUpdateApp();
        }

        private void CheckUpdateApp()
        {
            var isNewVersion = _updateAppService.IsNewVersion();

            this.Dispatcher.Invoke(() =>
            {
                if (isNewVersion)
                {
                    var result = CustomMessageBox.ShowYesNo(Constants.Message.App.Info.CheckUpdateWindow.CHECK_NEW_VERSION_POPUP, "更新プログラムを確認", "はい", "いいえ", MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        _updateAppService.ShowProgressBarUpdate();
                        _updateAppService.DownLoadAndUpdate();
                    }
                }
            });
        }

        private string Login(string userId, string password, out string message)
        {
            message = string.Empty;
            var logs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.API);
            string methodName = nameof(Login);
            string output = string.Empty;

            try
            {
                var machine = Environment.MachineName;
                HttpClient client = new HttpClient();
                var jsonObject = new
                {
                    LoginId = userId,
                    Password = password
                };
                var jsonString = JsonConvert.SerializeObject(jsonObject);
                HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

#if DEBUG
                HttpResponseMessage response = client.PostAsync(_config.LoginEndpointDev, content).Result;
#else
                HttpResponseMessage response = client.PostAsync(_config.LoginEndpointStg, content).Result;
#endif
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogDebug(this, methodName, "Input", $"userId: {userId}, password: {password}"),
                Constants.Logs.LogType.DEBUG));
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogInfo(this, methodName, $"Request from Machine: {machine}", $"Status Code: {response.StatusCode}"),
                Constants.Logs.LogType.INFO));

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var objectType = new
                    {
                        Data = new
                        {
                            Token = string.Empty
                        }
                    };
                    var obj = JsonConvert.DeserializeAnonymousType(json, objectType);

                    if (obj == null || obj?.Data == null || string.IsNullOrEmpty(obj?.Data.Token))
                        return string.Empty;

                    output = obj.Data.Token;
                }

                return output;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogException(this, methodName, $"Exception: {ex.Message}", $"Stack Trace: {ex.StackTrace}"),
                Constants.Logs.LogType.EXCEPTION));
                return output;
            }
            finally
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogDebug(this, methodName, "Output", $" Result: {output}"),
                Constants.Logs.LogType.DEBUG));
                _logService.WriteLog(logs);
            }
        }

        private bool SaveIniFile(string token, out List<WriteLogRequest> writeLogs)
        {
            writeLogs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            string methodName = nameof(SaveIniFile);
            bool output = false;
            try
            {
                writeLogs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogDebug(this, methodName, "Input", $"token: {token}"),
                Constants.Logs.LogType.DEBUG));
                var settingFile = AppDomain.CurrentDomain.BaseDirectory + "setting.ini";

                if (File.Exists(settingFile))
                {
                    File.Delete(settingFile);
                }

                var iniFile = new IniFile(settingFile);
                output = iniFile.SetValue("Online", "Token", token);
                return output;
            }
            catch (Exception ex)
            {
                writeLogs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogException(this, methodName, $"Exception: {ex.Message}", $"Stack Trace: {ex.StackTrace}"),
                Constants.Logs.LogType.EXCEPTION));
                return output;
            }
            finally
            {
                writeLogs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogDebug(this, methodName, "Output", $" Result: {output}"),
                Constants.Logs.LogType.DEBUG));
            }
        }
    }
}