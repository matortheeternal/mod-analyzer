using ModAnalyzer.Utils;
using System;
using System.IO;

namespace ModAnalyzer.Domain.Services {
    public static class LogService {
        private static bool Started;
        private static string Log;
        private static string FilePath;

        public static void StartLogging() {
            string fileName = string.Format("Log {0}.txt", DateTime.Now.ToString("MM-dd-yyyy"));
            FilePath = Path.Combine(PathExtensions.GetProgramPath(), "Logs", fileName);
            Started = true;
            Message("STARTED SESSION");
        }

        public static void Message(string message) {
            if (!Started) return;
            string formattedMessage = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), message);
            App.Current.Dispatcher.BeginInvoke((Action)(delegate {
                Log += formattedMessage + Environment.NewLine;
                Write(formattedMessage);
            }));
        }

        public static void Write(string message) {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            using (StreamWriter sw = File.AppendText(FilePath)) {
                sw.WriteLine(message);
            }
        }

        public static void GroupMessage(string group, string message) {
            Message(string.Format("<{0}> {1}", group.ToUpper(), message));
        }
    }
}