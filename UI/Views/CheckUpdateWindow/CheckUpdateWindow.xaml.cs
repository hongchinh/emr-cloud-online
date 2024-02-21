using log4net;
using log4net.Appender;
using OnlineService.Request;
using OnlineService.Services;
using OnlineService.Util;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OnlineService.UI.Views.CheckUpdateWindow
{
    /// <summary>
    /// Interaction logic for CheckUpdateWindow.xaml
    /// </summary>
    public partial class CheckUpdateWindow : Window
    {
        private readonly UpdateAppService _updateAppService = new UpdateAppService();
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        private readonly ApiService _logService = new ApiService();
        public CheckUpdateWindow()
        {
            InitializeComponent();
            lbNew.Visibility = Visibility.Hidden;
            lbNewVersion.Visibility = Visibility.Hidden;
            btUpdate.IsEnabled = false;
            CheckUpdate();
        }
        public void CheckUpdate()
        {
            try
            {
                Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                if (NetworkUtil.IsInternetConnected())
                {
                    var isNewVersion = _updateAppService.IsNewVersion();
                    this.Dispatcher.Invoke(() =>
                    {
                        lbCurrentVersion.Content = _updateAppService.GetCurrentFileVerSion();
                        if (isNewVersion)
                        {
                            lbHeader.Content = Constants.Message.App.Info.CheckUpdateWindow.CHECK_NEW_VERSION;
                            lbNewVersion.Content = _updateAppService.GetNewFileVerSion();
                            btUpdate.IsEnabled = true;
                            lbNew.Visibility = Visibility.Visible;
                            lbNewVersion.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            lbHeader.Content = Constants.Message.App.Info.CheckUpdateWindow.CURRENT_VERSION;
                            btUpdate.IsEnabled = false;
                            lbNew.Visibility = Visibility.Hidden;
                            lbNewVersion.Visibility = Visibility.Hidden;
                        }
                    });
                }
                else
                {
                    var logs = new List<WriteLogRequest>();                    
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                        Log.WriteLogInfo(this, nameof(CheckUpdate), Constants.Message.Log.Error.App.CheckUpdate.LOST_CONNECTION),
                        Constants.Logs.LogType.ERROR));
                    _logService.WriteLog(logs);
                    NetworkUtil.ShowMessageBoxNetwork();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(Constants.Message.App.Error.CheckUpdateWindow.LOST_CONNECTION_ENG))
                {
                    var logs = new List<WriteLogRequest>();
                    Log.SwitchFileAppender(_appender, Constants.Logs.APP);
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                        Log.WriteLogException(this, nameof(CheckUpdate), $"Exception:{ex.Message}", ""),
                        Constants.Logs.LogType.EXCEPTION));
                    _logService.WriteLog(logs);
                    NetworkUtil.ShowMessageBoxNetwork();
                }
            }

        }

        private void btUpdate_Click(object sender, RoutedEventArgs e)
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            if (NetworkUtil.IsInternetConnected())
            {
                btUpdate.IsEnabled = false;
                _updateAppService.ShowProgressBarUpdate();
                _updateAppService.DownLoadAndUpdate();
            }
            else
            {
                var logs = new List<WriteLogRequest>();                
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                    Log.WriteLogInfo(this, nameof(CheckUpdate), Constants.Message.Log.Error.App.CheckUpdate.LOST_CONNECTION),
                    Constants.Logs.LogType.ERROR));
                _logService.WriteLog(logs);
                NetworkUtil.ShowMessageBoxNetwork();
            }
        }
    }
}
