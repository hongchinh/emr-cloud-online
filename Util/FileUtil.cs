using log4net;
using log4net.Appender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace OnlineService.Util
{
    public static class FileUtil
    {
        private static readonly FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        private static readonly string _className = $"{Assembly.GetCallingAssembly().GetName().Name}.{nameof(FileUtil)}";

        public static bool ExistMakeDir(string path)
        {
            Directory.CreateDirectory(path);
            return IsDirectoryExisting(path);
        }

        public static bool IsDirectoryExisting(string directoryName, int timeout = 5000)
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);

            string functionName = nameof(IsDirectoryExisting);
            bool output = false;
            try
            {
                Log.WriteLogDebug(_className, functionName, "Input", $"directoryName: {directoryName}");
                Log.WriteLogInfo(_className, functionName, "Check Directory.Exists", $"directoryName: {directoryName}");
                output = Directory.Exists(directoryName);
                return output;
            }
            catch (Exception ex)
            {
                Log.WriteLogException(_className, functionName, $"Message: {ex.Message}", $"directoryName: {directoryName}", $"Stack Trace: {ex.StackTrace}");
                return output;
            }
            finally
            {
                Log.WriteLogDebug(_className, functionName, "Output", $"Result: {output}");
            }
        }

        public static bool SaveTextFile(string path, bool append, string encode, List<string> lines, int maxRetry, int interval)
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            string functionName = nameof(SaveTextFile);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.GetEncoding("Shift_JIS");

            bool success = false;
            int retry = 0;

            if (maxRetry <= 0)
            {
                maxRetry = 1;
            }

            Log.WriteLogDebug(_className, functionName, "Input", $"path: {path}, append: {append}, encode: {encode}, lines.Count: {lines.Count}, maxRetry: {maxRetry}, interval: {interval}");

            while (retry < maxRetry && success == false)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(path, append, Encoding.GetEncoding(encode)))
                    {
                        foreach (string line in lines)
                        {
                            sw.WriteLine(line);
                        }
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    retry++;
                    Log.WriteLogException(_className, functionName, $"Message: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                    Thread.Sleep(interval);
                }
            }

            Log.WriteLogDebug(_className, functionName, "Output", $"Result: {success}");
            return success;
        }

        public static bool IsFileExisting(string fileName, int timeout = 5000)
        {
            bool isTimeOut = false;
            return IsFileExistingWithReason(fileName, ref isTimeOut, timeout);
        }

        public static bool IsFileExistingWithReason(string fileName, ref bool isTimeOut, int timeout = 5000)
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            string functionName = nameof(IsFileExistingWithReason);
            bool output = false;

            try
            {
                Log.WriteLogDebug(_className, functionName, "Input", $"fileName: {fileName}");
                Log.WriteLogInfo(_className, functionName, "Call File.Exists: " + fileName);
                output = File.Exists(fileName);
                return output;
            }
            catch (Exception ex)
            {
                Log.WriteLogException(_className, functionName, $"Message: {ex.Message}", $"fileName: {fileName}", $"Stack Trace: {ex.StackTrace}");
                return output;
            }
            finally
            {
                Log.WriteLogDebug(_className, functionName, "Output", $"Result: {output}");
            }
        }

        public static bool FileCopy(string source, string dest, int maxRetry, int interval)
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.SIGNALR);
            string functionName = nameof(FileCopy);
            bool successCopy = false;
            int retryCount = 0;
            Log.WriteLogDebug(_className, functionName, "Input", $"source: {source}, dest: {dest}, maxRetry: {maxRetry}, interval: {interval}");

            while (successCopy == false && retryCount < maxRetry)
            {
                try
                {
                    File.Copy(source, dest, true);
                    successCopy = true;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Log.WriteLogException(_className, functionName, $"Message: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                    Thread.Sleep(interval);
                }
            }

            Log.WriteLogDebug(_className, functionName, "Output", $"Result: {successCopy}");
            return successCopy;
        }

        public static void DeleteFile(string filePath)
        {
            string functionName = nameof(DeleteFile);
            try
            {
                var checkFile = IsFileExisting(filePath);
                if (checkFile)
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLogException(_className, functionName, $"Message: {ex.Message}", $"fileName: {filePath}", $"Stack Trace: {ex.StackTrace}");
            }
        }
    }
}