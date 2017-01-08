using ModAnalyzer.Utils;
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
        private readonly string[] archiveExtensions = { ".BA2", ".BSA" };
        public List<MissingMaster> MissingMasters;
        private List<string> PluginPaths;
        private List<ModOption> ArchiveModOptions;
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

        // Background job to extract an archive
        private void BackgroundWork(object sender, DoWorkEventArgs e) {
            ArchiveModOptions = e.Argument as List<ModOption>;

            try {
                // find plugins and BSAs from each archive, extract them, and check for missing masters
                FindEntriesToExtract();
                ExtractEntries();
                GetMissingMasters();
            }
            catch (Exception x) {
                _backgroundWorker.ReportMessage(Environment.NewLine + x.Message, false);
                _backgroundWorker.ReportMessage(x.StackTrace, false);
                _backgroundWorker.ReportMessage("Extraction failed.", true);
            }

            // tell the view model we're done extracting things
            ArchivesExtracted?.Invoke(this, new ArchivesExtractedEventArgs(MissingMasters));
        }

        private void AddMissingMasterEntry(string missingMasterFile, string pluginFileName) {
            MissingMaster existingEntry = MissingMasters.Find(x => x.FileName == missingMasterFile);
            if (existingEntry != null) {
                existingEntry.AddRequiredBy(pluginFileName);
            } else {
                MissingMasters.Add(new MissingMaster(missingMasterFile, pluginFileName));
            }
        }

        private void GetPluginMissingMasters(string pluginPath) {
            List<string> missingMasterFiles = _pluginAnalyzer.GetMissingMasterFiles(pluginPath);
            string pluginFileName = Path.GetFileName(pluginPath);
            foreach (string missingMasterFile in missingMasterFiles) {
                AddMissingMasterEntry(missingMasterFile, pluginFileName);
            }
        }

        private void GetMissingMasters() {
            foreach (ModOption archiveModOption in ArchiveModOptions) {
                if (archiveModOption.PluginPaths.Count > 0) CreatePluginAnalyzer();
                foreach (string pluginPath in archiveModOption.PluginPaths) {
                    GetPluginMissingMasters(pluginPath);
                }
            }
        }

        private void ExtractEntry(ModOption archiveModOption, IArchiveEntry entry) {
            string destinationPath = archiveModOption.GetExtractedEntryPath(entry);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
            entry.WriteToFile(destinationPath);
            string entryExt = Path.GetExtension(destinationPath);
            if (pluginExtensions.Contains(entryExt, StringComparer.OrdinalIgnoreCase)) {
                archiveModOption.PluginPaths.Add(destinationPath);
            } else if (archiveExtensions.Contains(entryExt, StringComparer.OrdinalIgnoreCase)) {
                archiveModOption.ArchivePaths.Add(destinationPath);
            }
        }

        private void FindEntriesToExtract() {
            foreach (ModOption archiveModOption in ArchiveModOptions) {
                IArchive archive = archiveModOption.Archive;
                foreach (IArchiveEntry modArchiveEntry in archive.Entries) {
                    string entryExt = modArchiveEntry.GetEntryExtension();
                    if (jobFileExtensions.Contains(entryExt, StringComparer.OrdinalIgnoreCase)) {
                        archiveModOption.EntriesToExtract.Add(modArchiveEntry);
                    }
                }
            }
        }

        private void ExtractEntries() {
            int total = 0, tracker = 0;
            foreach (ModOption archiveModOption in ArchiveModOptions)
                total += archiveModOption.EntriesToExtract.Count;
            foreach (ModOption archiveModOption in ArchiveModOptions) {
                for (int i = 0; i < archiveModOption.EntriesToExtract.Count; i++) {
                    try {
                        tracker += 1;
                        IArchiveEntry entryToExtract = archiveModOption.EntriesToExtract[i];
                        string fileName = Path.GetFileName(entryToExtract.GetPath());
                        string countString = " (" + tracker + "/" + total + ")";
                        _backgroundWorker.ReportMessage("Extracting " + fileName + countString, true);
                        ExtractEntry(archiveModOption, entryToExtract);
                    } catch (Exception x) {
                        _backgroundWorker.ReportMessage(x.Message, false);
                    }
                }
            }
        }

        public void ExtractArchives(List<ModOption> archiveModOptions) {
            _backgroundWorker.RunWorkerAsync(archiveModOptions);
        }
    }
}
