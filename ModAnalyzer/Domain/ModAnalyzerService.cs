using System;
using System.ComponentModel;
using System.IO;

namespace ModAnalyzer.Domain
{
    public class ModAnalyzerService
    {
        private readonly BackgroundWorker _backgroundWorker;
        private readonly AssetArchiveAnalyzer _assetArchiveAnalyzer;
        private readonly PluginAnalyzer _pluginAnalyzer;

        public EventHandler<MessageReportedEventArgs> MessageReported;
        
        public ModAnalyzerService()
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;

            _assetArchiveAnalyzer = new AssetArchiveAnalyzer(_backgroundWorker);
            _pluginAnalyzer = new PluginAnalyzer(_backgroundWorker);

            Directory.CreateDirectory("output");
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MessageReportedEventArgs eventArgs = e.UserState as MessageReportedEventArgs;

            MessageReported?.Invoke(this, eventArgs);
        }

        public void AnalyzeMod(string modArchivePath)
        {
            try
            {
                
                _assetArchiveAnalyzerBackgroundWorker.RunWorkerAsync(message.FilePath);
            }
            catch (Exception e)
            {
                _backgroundWorker.ReportProgress(new MessageReportedEventArgs(MessageType.LogMessage, "Failed to analyze archive."));
                RaiseMessageReported(new MessageReportedEventArgs(MessageType.LogMessage, "Exception:" + e.Message));
            }
        }
    }
}
