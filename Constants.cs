using System;
using System.Windows.Forms;

namespace OnlineService
{
    public static class Constants
    {
        public static readonly DateTime JAPAN_TIME = DateTime.UtcNow.AddHours(9);

        public static class Message
        {
            public const string SUCCESS = "success";
            public const string NOT_FOUND = "not found";

            public static class App
            {
                public static class Info
                {
                    public static class SettingWindow
                    {
                        public static class ButtonCopy
                        {
                            public const string COPY = "ポート番号はクリップボードにコピーしました。";
                        }

                        public static class ButtonConfirm
                        {
                            public const string PORT_IN_USE = "ポート番号{0}で稼働中";
                        }
                    }
                    public static class CheckUpdateWindow
                    {
                        public const string UPDATE_PROGRESS_TITLE = "アップデート中...";
                        public const string CHECK_NEW_VERSION_POPUP = "最新版が見つかりました。今すぐアップデートしますか？";
                        public const string CHECK_NEW_VERSION = "最新版が見つかりました。\r\n今すぐアップデートしますか？";
                        public const string CURRENT_VERSION = "最新状態を保っています。";
                    }
                }

                public static class Error
                {
                    public static class SettingWindow
                    {
                        public static class ButtonCopy
                        {
                            public const string COPY = "予期しないエラーが発生したため、ポート番号はクリップボードコピーできませんでした。";
                        }

                        public static class ButtonConfirm
                        {
                            public const string CHECKING_PORT_NUMBER = "予期しないエラーが発生しました。";
                            public const string PORT_IN_USE = "指定したポート番号が別のアプリに既に利用されているため、設定できません。別のポート番号を指定してください。";
                            public const string PORT_OUT_OF_RANGE = "指定したポート番号は範囲外です。5001から65535の中、一つを選んでください。";
                            public const string ESTABLISHING_CONNECTION = "予期しないエラーが発生しました。ポート番号の保存ができません。";
                            public const string CAPTION = "エラー";
                            public const string BUTTON_TEXT = "閉じる";
                        }
                    }
                    public static class LoginWindow
                    {
                        public const string EMPTY_USER_ID = "ユーザーIDを入力してくささい。";
                        public const string EMPTY_PW = "パスワードを入力してください。";
                        public const string INVALID_CREDENTIALS = "ユーザーIDまたはパスワードが違います。再入力してください。";
                        public const string ERROR_LOGIN = "予期しないエラーが発生しました。ログインができません。";
                        public const string LOGIN_SUCCESS = "Login successful";
                        public const string LOGOUT_SUCCESS = "Logout successful";

                        public static class LoginButton
                        {
                            public const string EMPTY_USER_ID = "User ID is null or empty";
                            public const string EMPTY_PW = "Password is null or empty";
                            public const string INVALID_CREDENTIALS = "Invalid Credentials";
                            public const string ERROR_SAVE_INI = "An error occurred while saving setting.ini file";
                        }

                    }
                    public static class CheckUpdateWindow
                    {
                        public const string LOST_CONNECTION_DOWNLOAD = "ネットワークエラーのため、データーダウンロードが失敗しました。";
                        public const string LOST_CONNECTION = "インターネット接続をご確認ください。";
                        public const string LOST_CONNECTION_ENG = "One or more errors occurred. (No such host is known. (smarkarte-app.s3.ap-southeast-1.amazonaws.com:443))";
                    }
                }
            }

            public static class Web
            {
                public static class Info
                {
                }

                public static class Error
                {
                    public static class Version
                    {
                        public const string GETTING = "スマクリアプリのバージョン情報の取得に失敗しました。後程やり直してください。";
                        public const string CHECK_NEW_VERSION = "スマクリアプリの最新バージョンをリリースしました。お手数ですが、スマクリアプリの最新バージョンにアップデートをお願いいたします。";
                    }

                    public static class File
                    {
                        public const string REQUEST_NULL = "リクエストされた内容に誤りがあります。管理者に連絡してください。";
                        public const string GETTING = "スマクリアプリ側に予期しないエラーが発生しました。管理者に連絡してください。";
                        public const string EMPTY_LIST = "リクエストされた内容に誤りがあります。管理者に連絡してください。";
                        public const string DUPLICATE_FILE = "リクエストされた内容に誤りがあります。管理者に連絡してください。";
                        public const string MOVING = "スマクリアプリ側に予期しないエラーが発生しました。管理者に連絡してください。";
                        public const string EMPTY_NAME = "リクエストされた内容に誤りがあります。管理者に連絡してください。";
                        public const string WRONG_FORMAT_NAME = "リクエストされた内容に誤りがあります。管理者に連絡してください。";
                        public const string CREATING = "スマクリアプリ側に予期しないエラーが発生しました。管理者に連絡してください。";
                        public const string NOT_DETECTED = "タイムアウト";
                    }

                    public static class Path
                    {
                        public static readonly string EMPTY = "パスの設定に問題があるため、資格確認はできません。設定を見直してください。";
                        public static readonly string NOT_EXIST = "資格確認に失敗しました。照会先のフォルダーが存在しません。パスの設定を見直してください。";
                    }

                    public static class Api
                    {
                        public const string INVALID_TOKEN = "リクエストされた内容に誤りがあります。管理者に連絡してください。";
                        public const string UNABLE_TO_CONNECT = "スマクリアプリはサーバーに接続できません。管理者に連絡してください。";
                        public const string UNABLE_TO_CONNECT_ENG = "One or more errors occurred. (No connection could be made because the target machine actively refused it";
                    }

                    public static class ScreenCode
                    {
                        public const string INVALID = "リクエストされた内容に誤りがあります。管理者に連絡してください。";
                    }
                }
            }

            public static class Log
            {
                public static class Debug
                {
                    public const string INPUT = "Input";
                    public const string OUTPUT = "Output";
                } 

                public static class Info
                {
                    public static class SignalR
                    {
                        public static class Path
                        {
                            public const string LIST = "List paths";
                        }
                    }

                    public static class App
                    {
                        public static class SettingWindow
                        {
                            public const string SHOW = "Setting Window is showing";

                            public static class ButtonSuggest
                            {
                                public const string SUGGEST = "Suggest Free Port";
                            }

                            public static class ButtonConfirm
                            {
                                public const string PORT_SUCCESSFUL = "Port setup successful";
                            }
                        }

                        public static class MainWindow
                        {
                            public const string SHOW = "Main Window is showing";
                        }
                        public static class CheckUpdateWindow
                        {
                            public const string SHOW = "CheckUpdate Window is showing";
                        }
                        public static class ProgressUpdateWindow
                        {
                            public const string SHOW = "ProgressUpdate Window is showing";
                        }
                        public static class BackgroundWork
                        {
                            public const string CheckUpdate = "Background work check update version";
                        }
                        public static class CheckUpdate
                        {
                            public const string CHECK_VERSION_FROM_WEB = "Start checking for updates from the web";
                        }
                    }
                }

                public static class Error
                {
                    public static class SignalR
                    {
                        public static class File
                        {
                            public const string REQUEST_NULL = "Request object is not set to an instance";
                            public const string GETTING = "An error occured while getting file";
                            public const string EMPTY_LIST = "List files is null or empty";
                            public const string DUPLICATE_FILE = "Duplicate file";
                            public const string EMPTY_NAME = "File name is null or empty";
                            public const string WRONG_FORMAT_NAME = "File name is not in the correct format";
                            public const string CREATING = "An error occured while creating file";
                            public const string NOT_DETECTED = "No new files detected";
                            public const string NOT_EXIST = "File does not exist";
                            public const string DESERIALIZE = "An error occurred while deserializing json";
                            public const string ROOT_ELEMENT = "An error occurred while getting root element";
                        }

                        public static class Path
                        {
                            public const string EMPTY_LIST = "List paths is empty";
                            public const string NOT_EXIST = "Path does not exist";
                            public const string REQ_EMPTY = "Request path is empty";
                            public const string REQ_NOT_EXIST = "Request path does not exist";
                            public const string RES_EMPTY = "Response path is empty";
                            public const string RES_NOT_EXIST = "Response path does not exist";
                        }

                        public static class Api
                        {
                            public const string INVALID_TOKEN = "Invalid Token";
                        }

                        public static class ScreenCode
                        {
                            public const string INVALID = "Screen Code is invalid";
                        }
                    }

                    public static class App
                    {
                        public static class SettingWindow
                        {
                            public static class ButtonConfirm
                            {
                                public const string PARSE = "Error Parse";
                                public const string NULL_URL = "Base Url is null";
                                public const string PORT_IN_USE = "This port number is in use";
                                public const string PORT_OUT_OF_RANGE = "This port number is out of range";
                                public const string REWRITE_PORT = "Error rewrite to port number";
                            }
                        }   
                        public static class CheckUpdate
                        {
                            public const string LOST_CONNECTION_DOWNLOAD = "Lost network connection during file download.";
                            public const string LOST_CONNECTION = "No network connection.";
                            public const string OLD_VERSION = "The app has not been updated.";
                        }
                    }

                    public static class Json
                    {
                        public const string EMPTY = "Json string is empty";
                    }
                }

                public static class Exception
                {
                    public const string MESSAGE = "Message: {0}";
                    public const string STACK_TRACE = "Stack Trace: {0}";
                }
            }
        }

        public static class PathConf
        {
            public const int GRPCD_101 = 101;
            public const int GRPCD_102 = 102;
            public const int GRPCD_103 = 103;
            public const int GRPCD_54 = 54;
        }

        public static class ScreenCode
        {
            public const string PATIENT_INFO = "01000000";
            public const string RECEPTION = "00200000";
            public const string VISITING = "00030000";
            public const string MEDICAL = "02000000";
            public const string MAIN_MENU = "00040000";
        }

        public static class Tags
        {
            public static class Open
            {
                public const string ARBITRARY_FILE_IDENTIFIER = "<ArbitraryFileIdentifier>";
                public const string RECEPTION_NUMBER = "<ReceptionNumber>";
            }

            public static class Close
            {
                public const string ARBITRARY_FILE_IDENTIFIER = "</ArbitraryFileIdentifier>";
                public const string RECEPTION_NUMBER = "</ReceptionNumber>";
            }
        }

        public static class Logs
        {
            public static readonly string APP = "logs\\" + JAPAN_TIME.ToString("yyyyMMdd") + "\\app.log";
            public static readonly string SIGNALR = "logs\\" + JAPAN_TIME.ToString("yyyyMMdd") + "\\signalR.log";
            public static readonly string API = "logs\\" + JAPAN_TIME.ToString("yyyyMMdd") + "\\api.log";
            public static class EventCd
            {
                public const string APP = "App";
                public const string SIGNALR = "SignalR";
                public const string API = "Api";
            }
            public static class LogType
            {
                public const string INFO = "INFO";
                public const string DEBUG = "DEBUG";
                public const string ERROR = "ERROR";
                public const string EXCEPTION = "EXCEPTION";
            }
        }

        public static class Paths
        {
            public static class KensaIrai
            {
                public static readonly string ROOT = AppDomain.CurrentDomain.BaseDirectory + "Root";
                public static readonly string TEMPS = AppDomain.CurrentDomain.BaseDirectory + "Temps";
                public static readonly string ODRKENSAIRAI = ROOT + @$"\KensaIrai\" + JAPAN_TIME.ToString("yyyyMMdd");
                public static readonly string ODRKENSAIRAI_TEMP_PATH = TEMPS + @$"\KensaIrai\" + JAPAN_TIME.ToString("yyyyMMdd");
            }
        }
    }
}