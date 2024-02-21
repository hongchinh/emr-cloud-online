using log4net;
using log4net.Appender;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace OnlineService.Util
{
    public class IniFile
    {
        private static readonly FileAppender _appender = (FileAppender)LogManager.GetRepository().GetAppenders()[0];
        private static readonly string _className = $"{Assembly.GetCallingAssembly().GetName().Name}.{nameof(IniFile)}";
        private string _fileName;

        public IniFile(string fileName)
        {
            _fileName = fileName;
        }

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
          string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        public string GetValue(string section, string key)
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            string methodName = nameof(GetValue);
            string output = string.Empty;

            try
            {
                Log.WriteLogDebug(_className, methodName, "Input", $"section: {section}, key: {key}");
                const int bufferSize = 1024;
                StringBuilder temp = new StringBuilder(bufferSize);
                GetPrivateProfileString(section, key, "", temp, bufferSize, _fileName);
                output = temp.ToString();
                return output;
            }
            catch (Exception ex)
            {
                Log.WriteLogException(_className, methodName, $"Message: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                return output;
            }
            finally
            {
                Log.WriteLogDebug(_className, methodName, "Output", $"Result: {output}");
            }
        }

        public bool SetValue(string section, string key, string value)
        {
            Log.SwitchFileAppender(_appender, Constants.Logs.APP);
            string methodName = nameof(SetValue);
            string output = string.Empty;

            try
            {
                Log.WriteLogDebug(_className, methodName, "Input", $"section: {section}, key: {key}, value: {value}");
                if (WritePrivateProfileString(section, key, value, _fileName) > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                Log.WriteLogException(_className, methodName, $"Message: {ex.Message}", $"Stack Trace: {ex.StackTrace}");
                return false;
            }
            finally
            {
                Log.WriteLogDebug(_className, methodName, "Output", $"Result: {output}");
            }
        }

        public bool IsKeyExists(string key, string section)
        {
            return GetValue(key, section).Length > 0;
        }
    }
}