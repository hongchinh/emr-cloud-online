using log4net;
using log4net.Appender;
using Microsoft.AspNetCore.SignalR;
using OnlineService.Request;
using OnlineService.UI.Views.CustomMessageBoxWindow;
using OnlineService.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OnlineService.Services
{
    public class UpdateAppService
    {
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        private readonly Handler _handler = new Handler();
        protected JsonConfig _config = new JsonConfig();
        private readonly ApiService _logService = new ApiService();
        private bool isCancellingDownload = false;
        public void DownLoadAndUpdate()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            WebClient web = new WebClient();
            web.DownloadFileAsync(new Uri(_config.PathAppCloud), AppDomain.CurrentDomain.BaseDirectory + "\\SmartKarteApp.exe");
            web.DownloadProgressChanged += Web_DownloadProgressChanged;
            web.DownloadFileCompleted += Web_DownloadFileCompleted;
        }
        public bool IsNewVersion()
        {
            try
            {
                if (!NetworkUtil.IsInternetConnected())
                {
                    Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                    Log.WriteLogInfo(this, nameof(IsNewVersion), Constants.Message.Log.Error.App.CheckUpdate.LOST_CONNECTION);
                    return false;
                }
                var newVersionUrl = new HttpClient().GetStringAsync(_config.PathVersionCloud).Result;
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                var currentVersion = fvi.FileVersion;

                var newVersion = newVersionUrl.Replace(".", "");
                currentVersion = currentVersion?.Replace(".", "");
                int newVersionInt, currentVersionInt;
                if (int.TryParse(newVersion, out newVersionInt) && int.TryParse(currentVersion, out currentVersionInt))
                {
                    if (newVersionInt > currentVersionInt)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                var logs = new List<WriteLogRequest>();
                Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                logs.Add(new WriteLogRequest(
                Constants.Logs.EventCd.APP,
                Log.WriteLogException(this, nameof(IsNewVersion), $"Exeption: " + ex.Message, $"StackTrace: " + ex.StackTrace),
                Constants.Logs.LogType.EXCEPTION));
                _logService.WriteLog(logs);
                return false;
            }


        }
        public string GetCurrentFileVerSion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var currentVersion = fvi.FileVersion;
            return !string.IsNullOrEmpty(currentVersion) ? currentVersion.ToString() : "";
        }
        public string GetNewFileVerSion()
        {
            if (!NetworkUtil.IsInternetConnected())
            {
                Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                Log.WriteLogInfo(this, nameof(IsNewVersion), Constants.Message.Log.Error.App.CheckUpdate.LOST_CONNECTION);
                return "";
            }
            var fileVersion = new HttpClient().GetStringAsync(_config.PathVersionCloud).Result;
            return !string.IsNullOrEmpty(fileVersion) ? fileVersion.ToString() : "";
        }
        private void Web_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (isCancellingDownload)
            {
                ((WebClient)sender).CancelAsync();
            }
        }
        private void StopDownloadFile()
        {
            isCancellingDownload = true;
        }
        private void Web_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                var logs = new List<WriteLogRequest>();
                Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, nameof(Web_DownloadProgressChanged), Constants.Message.Log.Error.App.CheckUpdate.LOST_CONNECTION_DOWNLOAD, $"sender: {sender}"),
                Constants.Logs.LogType.ERROR));
                _logService.WriteLog(logs);
                foreach (Window window in Application.Current.Windows)
                {                    
                    window.Hide();
                }
                MessageBox.Show(Constants.Message.App.Error.CheckUpdateWindow.LOST_CONNECTION_DOWNLOAD, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (!isCancellingDownload)
                {
                    CreateFileUpdate();
                    var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    if (mainWindow != null)
                    {
                        mainWindow.HideNotifyIcon();
                    }
                    InitRunScript();
                }
            }
            isCancellingDownload = false;
        }

        private void InitRunScript()
        {
            var logs = new List<WriteLogRequest>();
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\update.bat";
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogDebug(this, nameof(InitRunScript), "Input", $"path: {path}"),
                Constants.Logs.LogType.DEBUG));
            _logService.WriteLog(logs);
            Process p = new Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = "";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Verb = "runas";
            p.Start();
            Environment.Exit(1);

        }
        private void CreateFileUpdate()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string exePath = appDirectory + @"SmartKarteApp.exe";

            string batchContent = $@"
@echo off

Taskkill /f /im OnlineService.exe
:exit

""{exePath}"" /VERYSILENT

del ""{exePath}""

cd ""{appDirectory}""

start OnlineService.exe
";
            string batchFilePath = Path.Combine(appDirectory, "update.bat");

            try
            {
                File.WriteAllText(batchFilePath, batchContent);
            }
            catch (Exception ex)
            {
                var logs = new List<WriteLogRequest>();
                Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                    Log.WriteLogError(this, nameof(CreateFileUpdate), ex.Message),
                    Constants.Logs.LogType.EXCEPTION));
                _logService.WriteLog(logs);
            }
        }

        public void ShowProgressBarUpdate()
        {
            Window progressBarWindow = new Window
            {
                Width = 400,
                Height = 100,
                Title = Constants.Message.App.Info.CheckUpdateWindow.UPDATE_PROGRESS_TITLE,
                Topmost = true,
                ResizeMode = ResizeMode.NoResize,
                Content = new ProgressBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    IsIndeterminate = true,
                    Height = 10,
                    Margin = new Thickness(10),

                },
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            progressBarWindow.Closing += ProgressBarWindow_Closing;
            foreach (Window window in Application.Current.Windows)
            {
                if (window != progressBarWindow)
                {
                    window.Hide();
                }
            }
            var logs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogInfo(this, nameof(ShowProgressBarUpdate), Constants.Message.Log.Info.App.ProgressUpdateWindow.SHOW),
                Constants.Logs.LogType.INFO));
            _logService.WriteLog(logs);
            progressBarWindow.Show();
        }

        public async Task<bool> IsCheckNewVersionFromWeb(IHubCallerClients clients, string method)
        {
            var logs = new List<WriteLogRequest>();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogInfo(this, nameof(IsCheckNewVersionFromWeb), Constants.Message.Log.Info.App.CheckUpdate.CHECK_VERSION_FROM_WEB),
                Constants.Logs.LogType.INFO));

            if (IsNewVersion())
            {
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, nameof(IsCheckNewVersionFromWeb), Constants.Message.Log.Error.App.CheckUpdate.OLD_VERSION),
                Constants.Logs.LogType.ERROR));
                var response = _handler.Response(new object(), Constants.Message.Web.Error.Version.CHECK_NEW_VERSION, 400);
                await clients.Caller.SendAsync(method, response);
                var result = CustomMessageBox.ShowYesNo(Constants.Message.App.Info.CheckUpdateWindow.CHECK_NEW_VERSION_POPUP, "更新プログラムを確認", "はい", "いいえ", MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    ShowProgressBarUpdate();
                    DownLoadAndUpdate();
                }
                _logService.WriteLog(logs);
                return true;
            }
            _logService.WriteLog(logs);
            return false;

        }
        private void ProgressBarWindow_Closing(object sender, CancelEventArgs e)
        {
            StopDownloadFile();
        }
    }
}
