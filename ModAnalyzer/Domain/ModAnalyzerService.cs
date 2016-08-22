using ModAnalyzer.Utils;
using Newtonsoft.Json;
using SharpCompress.Archive;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ModAnalyzer.Domain
{
    // TODO: apply DRY to _backgroundWorker.ReportProgress
    public class ModAnalyzerService
    {
        private readonly BackgroundWorker _backgroundWorker;
        private readonly AssetArchiveAnalyzer _assetArchiveAnalyzer;
        private readonly PluginAnalyzer _pluginAnalyzer;
        private readonly ModAnalysis _modAnalysis;

        public event EventHandler<MessageReportedEventArgs> MessageReported;
        
        public ModAnalyzerService()
        {
            _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;

            _assetArchiveAnalyzer = new AssetArchiveAnalyzer(_backgroundWorker);
            _pluginAnalyzer = new PluginAnalyzer(_backgroundWorker);

            _modAnalysis = new ModAnalysis();

            Directory.CreateDirectory("output");
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> modArchivePaths = e.Argument as List<string>;

            foreach(string modArchivePath in modArchivePaths)
            {
                _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs("Analyzing " + Path.GetFileName(modArchivePath) + "..."));

                using (IArchive archive = ArchiveFactory.Open(modArchivePath))
                {
                    foreach (IArchiveEntry modArchiveEntry in archive.Entries)
                    {
                        if (modArchiveEntry.IsDirectory)
                            continue;

                        AnalyzeModArchiveEntry(modArchiveEntry);
                    }

                    SaveOutputFile(modArchivePath);
                }
            }
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MessageReportedEventArgs eventArgs = e.UserState as MessageReportedEventArgs;

            MessageReported?.Invoke(this, eventArgs);
        }

        public void AnalyzeMod(List<string> modArchivePaths)
        {
            _backgroundWorker.RunWorkerAsync(modArchivePaths);
        }

        private void AnalyzeModArchiveEntry(IArchiveEntry modArchiveEntry)
        {
            string entryPath = modArchiveEntry.GetEntryPath();
            _modAnalysis.assets.Add(entryPath);

            _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs(entryPath));

            switch (modArchiveEntry.GetEntryExtension())
            {
                case ".BA2":
                case ".BSA":
                    _modAnalysis.assets.AddRange(_assetArchiveAnalyzer.GetAssets(modArchiveEntry));
                    break;
                case ".ESP":
                case ".ESM":
                    PluginDump pluginDump = _pluginAnalyzer.GetPluginDump(modArchiveEntry);
                    if (pluginDump != null)
                        _modAnalysis.plugins.Add(_pluginAnalyzer.GetPluginDump(modArchiveEntry));
                    break;
            }
        }

        private void SaveOutputFile(string filePath)
        {
            string filename = Path.Combine("output", Path.GetFileNameWithoutExtension(filePath));

            _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateProgressMessageEventArgs("Saving JSON to " + filename + ".json..."));
            File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(_modAnalysis));
            _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateProgressMessageEventArgs("All done.  JSON file saved to " + filename + ".json"));
        }
    }
}
