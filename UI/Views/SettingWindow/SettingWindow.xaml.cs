using log4net;
using log4net.Appender;
using OnlineService.Extensions;
using OnlineService.Request;
using OnlineService.Services;
using OnlineService.UI.Views.CustomMessageBoxWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace OnlineService.UI.Views.SettingWindow
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        private FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        private readonly ApiService _logService = new ApiService();
        private readonly SignalRPortService _portService = new SignalRPortService();
        public SettingWindow()
        {
            InitializeComponent();
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtPort.Text);
            var port = Clipboard.GetText();

            if (!string.IsNullOrEmpty(port))
                MessageBox.Show(Constants.Message.App.Info.SettingWindow.ButtonCopy.COPY, string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(Constants.Message.App.Error.SettingWindow.ButtonCopy.COPY, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnSuggest_Click(object sender, RoutedEventArgs e)
        {
            var logs = new List<WriteLogRequest>();
            txtPort.Text = _portService.FreePort().ToString();
            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogInfo(this, nameof(btnSuggest_Click), Constants.Message.Log.Info.App.SettingWindow.ButtonSuggest.SUGGEST, $"port: {txtPort.Text}"),
            Constants.Logs.LogType.INFO));
            _logService.WriteLog(logs);
        }

        private async void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var logs = new List<WriteLogRequest>();
            var methodName = nameof(btnConfirm_Click);
            var isParsed = int.TryParse(txtPort.Text.ToString(), out int port);

            if (!isParsed)
            {
                CustomMessageBox.ShowOK(Constants.Message.App.Error.SettingWindow.ButtonConfirm.CHECKING_PORT_NUMBER, Constants.Message.App.Error.SettingWindow.ButtonConfirm.CAPTION, Constants.Message.App.Error.SettingWindow.ButtonConfirm.BUTTON_TEXT, MessageBoxImage.None);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.App.SettingWindow.ButtonConfirm.PARSE, $"port: {port}"),
                Constants.Logs.LogType.ERROR));
                _logService.WriteLog(logs);
                return;
            }

            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (mainWindow == null)
            {
                mainWindow = new MainWindow();
            }

            var webApplication = mainWindow?.WebApplication;
            var currentUrl = webApplication?.GetBaseUrl();

            if (currentUrl == null)
            {
                CustomMessageBox.ShowOK(Constants.Message.App.Error.SettingWindow.ButtonConfirm.CHECKING_PORT_NUMBER, Constants.Message.App.Error.SettingWindow.ButtonConfirm.CAPTION, Constants.Message.App.Error.SettingWindow.ButtonConfirm.BUTTON_TEXT, MessageBoxImage.None);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.App.SettingWindow.ButtonConfirm.NULL_URL, $"webApplication: {webApplication}"),
                Constants.Logs.LogType.ERROR));
                _logService.WriteLog(logs);
                return;
            }

            var uri = new Uri(currentUrl);
            var currentPort = uri.Port;

            if (port == currentPort)
            {
                var savePortDb = _logService.UpdateSignalRPort(port);
                if (!savePortDb)
                {
                    CustomMessageBox.ShowOK(Constants.Message.App.Error.SettingWindow.ButtonConfirm.ESTABLISHING_CONNECTION, Constants.Message.App.Error.SettingWindow.ButtonConfirm.CAPTION, Constants.Message.App.Error.SettingWindow.ButtonConfirm.BUTTON_TEXT, MessageBoxImage.None);
                    logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                    Log.WriteLogError(this, methodName, Constants.Message.Log.Error.App.SettingWindow.ButtonConfirm.REWRITE_PORT, $"port: {port}"),
                    Constants.Logs.LogType.ERROR));
                    _logService.WriteLog(logs);
                    return;
                }
                Close();
                return;
            }

            if (!_portService.IsFreePort(port))
            {
                CustomMessageBox.ShowOK(Constants.Message.App.Error.SettingWindow.ButtonConfirm.PORT_IN_USE, Constants.Message.App.Error.SettingWindow.ButtonConfirm.CAPTION, Constants.Message.App.Error.SettingWindow.ButtonConfirm.BUTTON_TEXT, MessageBoxImage.None);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.App.SettingWindow.ButtonConfirm.PORT_IN_USE, $"port: {port}"),
                Constants.Logs.LogType.ERROR));
                _logService.WriteLog(logs);
                return;
            }

            if (port < 5001 || port > 65535)
            {
                CustomMessageBox.ShowOK(Constants.Message.App.Error.SettingWindow.ButtonConfirm.PORT_OUT_OF_RANGE, Constants.Message.App.Error.SettingWindow.ButtonConfirm.CAPTION, Constants.Message.App.Error.SettingWindow.ButtonConfirm.BUTTON_TEXT, MessageBoxImage.None);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.App.SettingWindow.ButtonConfirm.PORT_OUT_OF_RANGE, $"port: {port}"),
                Constants.Logs.LogType.ERROR));
                _logService.WriteLog(logs);
                return;
            }
            var savePort = _logService.UpdateSignalRPort(port);
            if (!savePort)
            {
                CustomMessageBox.ShowOK(Constants.Message.App.Error.SettingWindow.ButtonConfirm.ESTABLISHING_CONNECTION, Constants.Message.App.Error.SettingWindow.ButtonConfirm.CAPTION, Constants.Message.App.Error.SettingWindow.ButtonConfirm.BUTTON_TEXT, MessageBoxImage.None);
                logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
                Log.WriteLogError(this, methodName, Constants.Message.Log.Error.App.SettingWindow.ButtonConfirm.REWRITE_PORT, $"port: {port}"),
                Constants.Logs.LogType.ERROR));
                _logService.WriteLog(logs);
                return;
            }

            logs.Add(new WriteLogRequest(Constants.Logs.EventCd.APP,
            Log.WriteLogInfo(this, methodName, Constants.Message.Log.Info.App.SettingWindow.ButtonConfirm.PORT_SUCCESSFUL, $"port: {port}"),
            Constants.Logs.LogType.INFO));
            _logService.WriteLog(logs);
            await mainWindow.StopSignalRServer();
            mainWindow.StartSignalRServer();

            lblPortInfo.Content = string.Format(Constants.Message.App.Info.SettingWindow.ButtonConfirm.PORT_IN_USE, port);
            Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }



        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}