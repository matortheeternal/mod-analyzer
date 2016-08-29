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
                ReportProgress("Analyzing " + Path.GetFileName(modArchivePath) + "...");

                using (IArchive archive = ArchiveFactory.Open(modArchivePath))
                {
                    if (IsFomodArchive(archive))
                    {
                        List<ModOption> fomodOptions = AnalyzeFomodArchive(archive);
                        _modAnalysis.ModOptions.AddRange(fomodOptions);
                    }
                    else
                    {
                        ModOption option = AnalyzeNormalArchive(archive);
                        _modAnalysis.ModOptions.Add(option);
                    }
                }
            }

            // TODO: This should get the name of the base mod option or something
            string filename = modArchivePaths[0];
            SaveOutputFile(filename);
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MessageReportedEventArgs eventArgs = e.UserState as MessageReportedEventArgs;

            MessageReported?.Invoke(this, eventArgs);
        }

        private void ReportProgress(string msg) 
        {
            MessageReportedEventArgs args = MessageReportedEventArgsFactory.CreateLogMessageEventArgs(msg);
            _backgroundWorker.ReportProgress(0, args);
        }

        private bool IsFomodArchive(IArchive archive) 
        {
            foreach (IArchiveEntry modArchiveEntry in archive.Entries) 
            {
                if (modArchiveEntry.Key == "fomod") 
                {
                    return true;
                }
            }
            return false;
        }

        private List<ModOption> AnalyzeFomodArchive(IArchive archive) 
        {
            List<ModOption> fomodOptions = new List<ModOption>();
            // TODO: FOMOD ARCHIVE ANALYSIS LOGIC
            // STEP 1: Find the fomod\info.xml file and extract it
            // STEP 2: Parse info.xml and determine what the mod options are
            // STEP 3: Loop through the archive's assets appending them to mod options per info.xml
            // STEP 4: Delete any options that have no assets in them
            return fomodOptions;
        }

        private ModOption AnalyzeNormalArchive(IArchive archive) {
            ModOption option = new ModOption();
            foreach (IArchiveEntry modArchiveEntry in archive.Entries) {
                if (modArchiveEntry.IsDirectory)
                    continue;

                AnalyzeModArchiveEntry(modArchiveEntry, option);
            }

            return option;
        }

        public void AnalyzeMod(List<string> modArchivePaths)
        {
            _backgroundWorker.RunWorkerAsync(modArchivePaths);
        }

        private void AnalyzeModArchiveEntry(IArchiveEntry modArchiveEntry, ModOption option)
        {
            string entryPath = modArchiveEntry.GetEntryPath();
            option.Assets.Add(entryPath);

            ReportProgress(entryPath);

            switch (modArchiveEntry.GetEntryExtension())
            {
                case ".BA2":
                case ".BSA":
                    List<String> assets = _assetArchiveAnalyzer.GetAssets(modArchiveEntry);
                    option.Assets.AddRange(assets);
                    break;
                case ".ESP":
                case ".ESM":
                    PluginDump pluginDump = _pluginAnalyzer.GetPluginDump(modArchiveEntry);
                    if (pluginDump != null)
                        option.Plugins.Add(pluginDump);
                    break;
            }
        }

        private void SaveOutputFile(string filePath)
        {
            string filename = Path.Combine("output", Path.GetFileNameWithoutExtension(filePath));

            ReportProgress("Saving JSON to " + filename + ".json...");
            File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(_modAnalysis));
            ReportProgress("All done.  JSON file saved to " + filename + ".json");
        }
    }
}
