using ModAnalyzer.Utils;
using System;
using System.IO;

namespace ModAnalyzer.Domain.Services {
    public static class LogService {
        private static bool Started;
        private static string FilePath;
        private static StreamWriter sw;

        public static void StartLogging() {
            string fileName = string.Format("Log {0}.txt", DateTime.Now.ToString("MM-dd-yyyy"));
            FilePath = Path.Combine(PathExtensions.GetProgramPath(), "Logs", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            sw = File.AppendText(FilePath);
            Started = true;
            Message("STARTED SESSION");
        }

        public static void StopLogging() {
            Message("ENDED SESSION");
            Started = false;
            sw.Close();
        }

        public static void Message(string message) {
            if (!Started) return;
            string formattedMessage = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), message);
            Write(formattedMessage);
        }

        public static void Write(string message) {
            sw.WriteLine(message);
        }

        public static void GroupMessage(string group, string message) {
            Message(string.Format("<{0}> {1}", group.ToUpper(), message));
        }
    }
}