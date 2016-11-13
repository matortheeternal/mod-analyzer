using System.ComponentModel;
using ModAnalyzer.Domain;

namespace ModAnalyzer.Utils
{
    public static class BackgroundWorkerExtensions
    {
        public static void ReportMessage(this BackgroundWorker backgroundWorker, string message, bool isStatusMessage = false)
        {
            backgroundWorker.ReportProgress(0, new MessageReportedEventArgs(message, isStatusMessage));
        }
    }
}
