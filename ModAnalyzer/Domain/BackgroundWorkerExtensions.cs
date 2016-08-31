using System.ComponentModel;

namespace ModAnalyzer.Domain {
    public static class BackgroundWorkerExtensions {
        public static void ReportMessage(this BackgroundWorker backgroundWorker, string message, bool isStatusMessage) {
            backgroundWorker.ReportProgress(0, new MessageReportedEventArgs(message, isStatusMessage));
        }
    }
}
