using log4net;
using log4net.Appender;
using System.Reflection;

namespace OnlineService
{
    public static class Log
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void WriteAppStart()
        {
            log.Info("System started");
        }

        public static void WriteAppShutdown()
        {
            log.Info("System end");
        }

        public static void WriteSignalRStart()
        {
            log.Info("SignalR Server started");
        }

        public static void WriteSignalRShutdown()
        {
            log.Info("SignalR Server end");
        }

        public static void WriteAPIStart()
        {
            log.Info("API started");
        }

        public static void WriteAPIShutdown()
        {
            log.Info("API end");
        }

        public static string WriteLogDebug(object className, string methodName, string msg, string param)
        {
            string message = string.Format(
                 "{0}.{1} - {2} - {3}",
                 className,
                 methodName,
                 msg,
                 param
             );

            log.Debug(message);
            return message;
        }

        public static string WriteLogInfo(object className, string methodName, string msg, string param)
        {
            string message = string.Format(
                 "{0}.{1} - {2} - {3}",
                 className,
                 methodName,
                 msg,
                 param
             );

            log.Info(message);
            return message;
        }

        public static string WriteLogInfo(object className, string methodName, string msg)
        {
            string message = string.Format(
                 "{0}.{1} - {2}",
                 className,
                 methodName,
                 msg
             );

            log.Info(message);
            return message;
        }

        public static string WriteLogError(object className, string methodName, string msg, string param)
        {
            string message = string.Format(
                 "{0}.{1} - {2} - {3}",
                 className,
                 methodName,
                 msg,
                 param
             );

            log.Error(message);
            return message;
        }

        public static string WriteLogError(object className, string methodName, string msg)
        {
            string message = string.Format(
                 "{0}.{1} - {2}",
                 className,
                 methodName,
                 msg
             );

            log.Error(message);
            return message;
        }

        public static string WriteLogException(object className, string methodName, string exceptionMessage, string param, string stackTrace)
        {
            string message = string.Format(
                "{0}.{1} - {2} - {3} - {4}",
                className,
                methodName,
                param,
                exceptionMessage,
                stackTrace
            );

            log.Error(message);
            return message;
        }

        public static string WriteLogException(object className, string methodName, string exceptionMessage, string stackTrace)
        {
            string message = string.Format(
                "{0}.{1} - {2} - {3}",
                className,
                methodName,
                exceptionMessage,
                stackTrace
            );

            log.Error(message);
            return message;
        }

        public static void SwitchFileAppender(FileAppender appender, string fileLog)
        {
            if (appender.File == fileLog)
                return;

            appender.File = fileLog;
            appender.ActivateOptions();
        }
    }
}