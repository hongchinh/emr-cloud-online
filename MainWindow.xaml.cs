using log4net;
using log4net.Appender;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OnlineService.Extensions;
using OnlineService.Hubs;
using OnlineService.Request;
using OnlineService.Services;
using OnlineService.UI.Views.CheckUpdateWindow;
using OnlineService.UI.Views.CustomMessageBoxWindow;
using OnlineService.UI.Views.LoginWindow;
using OnlineService.UI.Views.SettingWindow;
using OnlineService.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace OnlineService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebApplication? _webApp;
        private NotifyIcon _notifyIcon;
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        private readonly ApiService _logService = new ApiService();
        private readonly UpdateAppService _updateAppService = new UpdateAppService();
        private readonly SignalRPortService _portService = new SignalRPortService();

        public WebApplication? WebApplication { get => _webApp; }
        private DispatcherTimer autoRunTimer;
        private BackgroundWorkerService _backgroundWorkerService = new BackgroundWorkerService();
        bool messageBoxShownUpdate = false;
        private bool isDoWorkEventAttached = false;

        public MainWindow()
        {
            this.Hide();
            InitializeComponent();
            InitializeAutoRunTimer();

            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/reshot-icon-hospital.ico"),
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip(),
            };
            _notifyIcon.ContextMenuStrip.Items.Add("開く", null, ShowMainWindow);
            _notifyIcon.ContextMenuStrip.Items.Add("バージョン情報", null, ShowApplicationVersion);
            _notifyIcon.ContextMenuStrip.Items.Add("設定...", null, ShowSettings);
            _notifyIcon.ContextMenuStrip.Items.Add("更新プログラムを確認", null, ShowCheckUpdate);
            _notifyIcon.ContextMenuStrip.Items.Add("終了", null, CloseScreen);
            lblTimeVersion.Content = "ビルド日時: " + File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("HH:mm:ss dd/MM/yyyy");
            StartSignalRServer();
        }

        public bool IsWebApp()
        {
            return _webApp != null;
        }
        public void ShowNotifyIcon()
        {
            _notifyIcon.Visible = true;
        }
        public void HideNotifyIcon()
        {
            _notifyIcon.Visible = false;
        }
        private void ShowMainWindow(object? sender, EventArgs e)
        {
            var logs = new List<WriteLogRequest>();
            Show();
            Activate();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogInfo(this, nameof(ShowMainWindow), Constants.Message.Log.Info.App.MainWindow.SHOW, $"sender: {sender}"),
            Constants.Logs.LogType.INFO));
            _logService.WriteLog(logs);
        }

        private void ShowApplicationVersion(object? sender, EventArgs e)
        {
            var logs = new List<WriteLogRequest>();
            var methodName = nameof(ShowApplicationVersion);
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                System.Windows.Forms.MessageBox.Show($"{fvi.FileVersion}", "バージョン情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogInfo(this, methodName, $"File Version: {fvi.FileVersion}", $"sender: {sender}"),
                Constants.Logs.LogType.INFO));
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), $"sender: {sender}", string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                return;
            }
            finally
            {
                _logService.WriteLog(logs);
            }
        }

        private void ShowSettings(object? sender, EventArgs e)
        {
            var logs = new List<WriteLogRequest>();
            var methodName = nameof(ShowSettings);
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);

            if (!IsWindowOpen<SettingWindow>())
            {
                SettingWindow window = new();
                var url = _webApp?.GetBaseUrl();
                if (!string.IsNullOrEmpty(url))
                {
                    var uri = new Uri(url ?? string.Empty);
                    var portDbCurrent = _logService.GetSignalRPort();
                    if(portDbCurrent != uri.Port)
                    {
                        _logService.UpdateSignalRPort(uri.Port);
                    }                    
                    window.txtPort.Text = uri.Port.ToString();
                    window.lblPortInfo.Content = $"ポート番号{uri.Port}で稼働中";
                }
                else
                {
                    window.lblPortInfo.Content = "利用中ポートなし。\n資格確認利用できていない。";
                }
                window.Show();
            }
            else
            {
                var settingWindow = System.Windows.Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault();
                settingWindow?.Activate();
            }

            var wd = System.Windows.Application.Current.Windows.OfType<SettingWindow>().FirstOrDefault();
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogInfo(this, methodName, Constants.Message.Log.Info.App.SettingWindow.SHOW, $"sender: {sender}"),
            Constants.Logs.LogType.INFO));
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogInfo(this, methodName, $"Current Port: {wd?.txtPort.Text}", $"sender: {sender}"),
            Constants.Logs.LogType.INFO));
            _logService.WriteLog(logs);
        }

        private void CloseScreen(object? sender, EventArgs e)
        {
            _notifyIcon.Dispose();
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            Log.WriteSignalRShutdown();
            Log.SwitchFileAppender(_appender, Constants.Logs.API);
            Log.WriteAPIShutdown();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            Log.WriteAppShutdown();
            System.Windows.Application.Current.Shutdown();
        }

        public async Task StartSignalRServer()
        {
            var methodName = nameof(StartSignalRServer);
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            var logs = new List<WriteLogRequest>();
            try
            {
                var jsonConfig = new JsonConfig();
                var builder = WebApplication.CreateBuilder();
                builder.Services.AddSignalR();
                var port = _logService.GetSignalRPort();
                if (port > 0)
                {
                    var checkFreePort = _portService.IsFreePort(port);
                    if (!checkFreePort)
                    {
                        port = _portService.FreePort();
                        _logService.UpdateSignalRPort(port);
                    }
                }
                else if (port == 0)
                {
                    port = 11000;
                    _logService.UpdateSignalRPort(port);
                }
                builder.WebHost.UseUrls($"https://{jsonConfig.Host}:{port}");

                var currentDirectory = Directory.GetCurrentDirectory();
                var mergeCert = Path.Combine(currentDirectory, "Cert\\localhost.pfx");
                var certificatePath = Path.Combine(currentDirectory, "Cert\\localhost.crt");
                var privateKeyPath = Path.Combine(currentDirectory, "Cert\\localhost.key");

                builder.Configuration["Kestrel:Certificates:Default:Path"] = certificatePath;
                builder.Configuration["Kestrel:Certificates:Default:KeyPath"] = privateKeyPath;

                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(
                        builder =>
                        {
                            builder
                                .SetIsOriginAllowed(_ => true)
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials();
                        });
                });

                _webApp = builder.Build();

                _webApp.UseCors();

                _webApp.MapHub<CommonHub>("/hubs/common", configureOptions: configureOptions => { });
                _webApp.MapHub<PatientInfoHub>("/hubs/patient", configureOptions: configureOptions => { });
                _webApp.MapHub<VisitingHub>("/hubs/visiting", configureOptions: configureOptions => { });
                _webApp.MapHub<ReceptionHub>("/hubs/reception", configureOptions: configureOptions => { });
                _webApp.MapHub<MedicalHub>("/hubs/medical", configureOptions: configureOptions => { });
                _webApp.MapHub<MainMenuHub>("/hubs/mainmenu", configureOptions: configureOptions => { });

                Log.WriteSignalRStart();
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                "SignalR Server started",
                Constants.Logs.LogType.INFO));
                await _webApp.RunAsync();
            }
            catch (Exception ex)
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                Log.WriteLogException(this, methodName, string.Format(Constants.Message.Log.Exception.MESSAGE, ex.Message), string.Format(Constants.Message.Log.Exception.STACK_TRACE, ex.StackTrace)),
                Constants.Logs.LogType.EXCEPTION));
                throw;
            }
            finally
            {
                _logService.WriteLog(logs);
            }
        }

        public async Task StopSignalRServer()
        {
            var logs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

            if (_webApp == null)
            {
                Log.WriteSignalRShutdown();
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                "SignalR Server end",
                Constants.Logs.LogType.INFO));
                _logService.WriteLog(logs);
                return;
            }
            await _webApp.StopAsync();
            _webApp = null;
            Log.WriteSignalRShutdown();
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.SIGNALR,
                "SignalR Server end",
                Constants.Logs.LogType.INFO));
            _logService.WriteLog(logs);
        }

        private void OnClose(object sender, CancelEventArgs e)
        {
            var logs = new List<WriteLogRequest>();
            e.Cancel = true;
            Hide();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogInfo(this, nameof(ShowMainWindow), "Main Window is closed", $"sender: {sender}"),
            Constants.Logs.LogType.INFO));
            _logService.WriteLog(logs);
        }

        private bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? System.Windows.Application.Current.Windows.OfType<T>().Any()
               : System.Windows.Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window is LoginWindow)
                {
                    continue;
                }
                window.Close();
            }
            var logs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            Hide();
            var loginWindow = System.Windows.Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();

            if (loginWindow == null)
            {
                loginWindow = new LoginWindow();
            }
            _notifyIcon.Visible = false;
            var mainWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow?.StopSignalRServer();
            }
            _backgroundWorkerService.StopBackgroundWork();
            autoRunTimer.Stop();
            loginWindow.Show();
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogInfo(this, nameof(btnLogout_Click), Constants.Message.App.Error.LoginWindow.LOGOUT_SUCCESS),
            Constants.Logs.LogType.INFO));
            _logService.WriteLog(logs);
            RemoveIniFile();
        }

        private void ShowCheckUpdate(object? sender, EventArgs e)
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            var checkUpdateWindow = System.Windows.Application.Current.Windows.OfType<CheckUpdateWindow>().FirstOrDefault();
            if (NetworkUtil.IsInternetConnected())
            {
                var logs = new List<WriteLogRequest>();

                if (checkUpdateWindow == null)
                {
                    checkUpdateWindow = new CheckUpdateWindow();
                }
                else
                {
                    checkUpdateWindow.CheckUpdate();
                }
                checkUpdateWindow.Activate();
                checkUpdateWindow.Show();
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogInfo(this, "ShowCheckUpdate", Constants.Message.Log.Info.App.CheckUpdateWindow.SHOW, $"sender: {sender}"),
            Constants.Logs.LogType.INFO));
                _logService.WriteLog(logs);
            }
            else
            {
                checkUpdateWindow?.Hide();
                var logs = new List<WriteLogRequest>();
                Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                    Log.WriteLogError(this, nameof(ShowCheckUpdate), Constants.Message.Log.Error.App.CheckUpdate.LOST_CONNECTION),
                    Constants.Logs.LogType.ERROR));
                NetworkUtil.ShowMessageBoxNetwork();
            }
        }

        private bool RemoveIniFile()
        {
            var settingFile = AppDomain.CurrentDomain.BaseDirectory + "setting.ini";
            var output = false;

            if (File.Exists(settingFile))
            {
                File.Delete(settingFile);
                output = true;
            }

            return output;
        }

        public void InitializeAutoRunTimer()
        {
            autoRunTimer = new DispatcherTimer();
            autoRunTimer.Tick += new EventHandler(AutoRunTimer_Tick);
            autoRunTimer.Interval = TimeSpan.FromHours(1);
            autoRunTimer.Start();
        }

        private void AutoRunTimer_Tick(object sender, EventArgs e)
        {
            if (!isDoWorkEventAttached)
            {
                _backgroundWorkerService.DoWorkChanged += CheckUpdate_DoWork;
                _backgroundWorkerService.RunWorkerCompleted += CheckUpdate_RunWorkerCompleted;
            }
            if (!_backgroundWorkerService.IsBusy())
            {
                _backgroundWorkerService.StartBackgroundWork();
            }
        }

        private void CheckUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            var logs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            isDoWorkEventAttached = true;
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogInfo(this, "CheckUpdate_DoWork", Constants.Message.Log.Info.App.BackgroundWork.CheckUpdate, $"sender: {sender}"),
            Constants.Logs.LogType.INFO));
            _logService.WriteLog(logs);
            if (_backgroundWorkerService.IsCancellationPending())
            {
                e.Cancel = true;
                return;
            }
            var isNewVersion = _updateAppService.IsNewVersion();
            if (isNewVersion)
            {
                if (!messageBoxShownUpdate)
                {
                    messageBoxShownUpdate = true;
                    var result = CustomMessageBox.ShowYesNo(Constants.Message.App.Info.CheckUpdateWindow.CHECK_NEW_VERSION_POPUP, "更新プログラムを確認", "はい", "いいえ", MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        _updateAppService.DownLoadAndUpdate();
                    }
                    else
                    {
                        messageBoxShownUpdate = false;
                        autoRunTimer.Stop();
                    }

                }
            }
        }
        private void CheckUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {

            }
            else if (e.Error != null)
            {

            }
            else
            {

            }
        }
        public bool IsAutoRunTimerInitialized()
        {
            return autoRunTimer.IsEnabled;
        }
    }
}