using ModAnalyzer.Analysis.Events;
using ModAnalyzer.Domain.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace ModAnalyzer.Analysis.Services {
    public static class ModDump {
        public static event EventHandler<MessageReportedEventArgs> MessageReported;

        private static string GameArg {
            get {
                return GameService.currentGame.abbrName;
            }
        }
        private static string LastOutputLine;
        private static string LastError;
        private static Process Process;

        private static void StartModDump(string arguments) {
            LastError = string.Empty;
            LastOutputLine = string.Empty;
            Process = new Process();
            Process.StartInfo.FileName = "ModDump.exe";
            Process.StartInfo.Arguments = arguments;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardError = true;
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.StartInfo.CreateNoWindow = true;
            Process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            Process.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);
            Process.Start();
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();
            Process.WaitForExit();
            Thread.Sleep(100);
        }

        private static void OutputHandler(object sender, DataReceivedEventArgs e) {
            if (string.IsNullOrEmpty(e.Data)) return;
            App.Current.Dispatcher.BeginInvoke((Action)(delegate {
                MessageReported?.Invoke(sender, new MessageReportedEventArgs(e.Data, false));
            }));
            if (e.Data.Trim().Length > 0) {
                LastOutputLine = e.Data.Trim();
            }
        }

        private static void ErrorHandler(object sender, DataReceivedEventArgs e) {
            if (string.IsNullOrEmpty(e.Data)) return;
            App.Current.Dispatcher.BeginInvoke((Action)(delegate {
                MessageReported?.Invoke(sender, new MessageReportedEventArgs(e.Data, true));
            }));
            LastError = e.Data;
        }

        public static string DumpMasters(string filePath) {
            string arguments = string.Format("\"{0}\" -{1} -dumpMasters", filePath, GameArg);
            StartModDump(arguments);
            Process.WaitForExit();
            RaiseLastModDumpError();
            return LastOutputLine.Equals("NO MASTERS") ? "" : LastOutputLine;
        }

        public static void DumpPlugin(string filePath) {
            string arguments = string.Format("\"{0}\" -{1}", filePath, GameArg);
            StartModDump(arguments);
        }

        public static bool GetProcessExited() {
            return Process != null ? Process.HasExited : true;
        }

        public static void RaiseLastModDumpError() {
            if (!string.IsNullOrEmpty(LastError)) {
                throw new Exception(Process.StandardError.ReadToEnd().Split('\n').Last());
            }
        }

        public static string GetDumpResult() {
            Regex expr = new Regex("Saving JSON dump to: (.*)");
            if (expr.IsMatch(LastOutputLine)) {
                string dumpPath = expr.Match(LastOutputLine).Groups[1].Value;
                if (!File.Exists(dumpPath)) return string.Empty;
                return File.ReadAllText(dumpPath);
            }
            return string.Empty;
        }
    }
}