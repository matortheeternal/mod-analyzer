using ModAnalyzer.Utils;
using SevenZip;
using SharpCompress.Archive;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ModAnalyzer.Domain {
    class ArchiveService {
        private readonly BackgroundWorker _backgroundWorker;
        private PluginAnalyzer _pluginAnalyzer;
        private readonly string[] jobFileExtensions = { ".BA2", ".BSA", ".ESP", ".ESM" };
        private readonly string[] pluginExtensions = { ".ESP", ".ESM" };
        public List<MissingMaster> MissingMasters;
        private List<string> PluginPaths;
        private List<Tuple<ModOption, IArchiveEntry>> EntriesToExtract;
        public event EventHandler<MessageReportedEventArgs> MessageReported;
        public event EventHandler<ArchivesExtractedEventArgs> ArchivesExtracted;

        public ArchiveService() {
            // prepare background worker
            _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            _backgroundWorker.DoWork += BackgroundWork;
            _backgroundWorker.ProgressChanged += BackgroundProgress;

            // prepare lists
            MissingMasters = new List<MissingMaster>();
            PluginPaths = new List<string>();
            EntriesToExtract = new List<Tuple<ModOption,IArchiveEntry>>();

            // prepare directories
            Directory.CreateDirectory("extracted");
        }

        private void CreatePluginAnalyzer() {
            if (_pluginAnalyzer == null) {
                _pluginAnalyzer = new PluginAnalyzer(_backgroundWorker);
            }
        }

        private void BackgroundProgress(object sender, ProgressChangedEventArgs e) {
            MessageReportedEventArgs eventArgs = e.UserState as MessageReportedEventArgs;
            MessageReported?.Invoke(this, eventArgs);
        }

        // Background job to analyze a mod
        private void BackgroundWork(object sender, DoWorkEventArgs e) {
            List<ModOption> archiveModOptions = e.Argument as List<ModOption>;

            try {
                // find plugins and BSAs from each archive
                foreach (ModOption archiveModOption in archiveModOptions) {
                    _backgroundWorker.ReportMessage("Extracting " + archiveModOption.Name + "...", true);
                    FindEntriesToExtract(archiveModOption);
                }
                // extract entries
                ExtractEntries();
                // check for missing plugin masters
                if (PluginPaths.Count > 0) {
                    CreatePluginAnalyzer();
                    GetMissingMasters();
                }
            }
            catch (Exception x) {
                _backgroundWorker.ReportMessage(Environment.NewLine + x.Message, false);
                _backgroundWorker.ReportMessage(x.StackTrace, false);
                _backgroundWorker.ReportMessage("Extraction failed.", true);
            }

            // tell the view model we're done analyzing things
            ArchivesExtracted?.Invoke(this, new ArchivesExtractedEventArgs(MissingMasters));
        }

        private void GetMissingMasters() {
            foreach (string pluginPath in PluginPaths) {
                string pluginFileName = Path.GetFileName(pluginPath);
                List<string> missingMasterFiles = _pluginAnalyzer.GetMissingMasterFiles(pluginPath);
                foreach (string missingMasterFile in missingMasterFiles) {
                    MissingMaster existingEntry = MissingMasters.Find(x => x.FileName == missingMasterFile);
                    if (existingEntry != null) {
                        existingEntry.AddRequiredBy(pluginFileName);
                    } else {
                        MissingMasters.Add(new MissingMaster(missingMasterFile, pluginFileName));
                    }
                }
            }
        }

        private void ExtractEntry(ModOption archiveModOption, IArchiveEntry entry) {
            string destinationPath = Path.Combine("extracted", archiveModOption.Name, entry.GetPath());
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
            entry.WriteToFile(destinationPath);
            string entryExt = Path.GetExtension(destinationPath);
            if (pluginExtensions.Contains(entryExt, StringComparer.OrdinalIgnoreCase)) {
                PluginPaths.Add(destinationPath);
            }
        }

        private void FindEntriesToExtract(ModOption archiveModOption) {
            IArchive archive = archiveModOption.Archive;
            foreach (IArchiveEntry modArchiveEntry in archive.Entries) {
                string entryExt = modArchiveEntry.GetEntryExtension();
                if (jobFileExtensions.Contains(entryExt, StringComparer.OrdinalIgnoreCase)) {
                    EntriesToExtract.Add(new Tuple<ModOption,IArchiveEntry>(archiveModOption, modArchiveEntry));
                }
            }
        }

        private void ExtractEntries() {
            for (var i = 0; i < EntriesToExtract.Count; i++) {
                ModOption option = EntriesToExtract[i].Item1;
                IArchiveEntry entry = EntriesToExtract[i].Item2;
                string fileName = Path.GetFileName(entry.GetPath());
                string countString = " (" + (i + 1) + "/" + EntriesToExtract.Count + ")";
                _backgroundWorker.ReportMessage("Extracting " + fileName + countString, true);
                ExtractEntry(option, entry);
            }
        }

        public void ExtractArchives(List<ModOption> archiveModOptions) {
            _backgroundWorker.RunWorkerAsync(archiveModOptions);
        }
    }
}
