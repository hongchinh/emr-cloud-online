
using System;
using System.Net.NetworkInformation;
using System.Windows;

namespace OnlineService.Util
{
    public static class NetworkUtil
    {
        public static void ShowMessageBoxNetwork()
        {
            MessageBox.Show(Constants.Message.App.Error.CheckUpdateWindow.LOST_CONNECTION, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static bool IsInternetConnected()
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send("www.google.com");
                return (reply.Status == IPStatus.Success);
            }
            catch (Exception ex)
            {
                Log.WriteLogException("NetworkUtil", "IsInternetConnected", $"Exception: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
