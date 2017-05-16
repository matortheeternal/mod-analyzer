using ModAnalyzer.Utils;
using ModAnalyzer.Analysis.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SevenZipExtractor;
using ModAnalyzer.Analysis.Events;

namespace ModAnalyzer.Analysis.Services {
    public class ArchiveService {
        private readonly BackgroundWorker _backgroundWorker;
        private PluginAnalyzer _pluginAnalyzer;
        public List<MissingMaster> MissingMasters;
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
            ArchiveModOptions.ForEach(x => x.CloseArchive());

            try {
                // find plugins and BSAs from each archive, extract them, and check for missing masters
                FindEntries();
                ExtractEntries();
                GetMissingMasters();
            }
            catch (Exception x) {
                _backgroundWorker.ReportMessage(Environment.NewLine + x.Message, false);
                _backgroundWorker.ReportMessage(x.StackTrace, false);
                _backgroundWorker.ReportMessage("Extraction failed.", true);
            }

            // tell the view model we're done extracting things
            ArchiveModOptions.ForEach(x => x.CloseArchive());
            ArchivesExtracted?.Invoke(this, new ArchivesExtractedEventArgs(MissingMasters));
        }

        private void AddMissingMasterEntry(string missingMasterFile, string pluginFileName) {
            MissingMaster existingEntry = MissingMasters.Find(x => x.FileName == missingMasterFile);
            if (existingEntry != null) {
                existingEntry.AddRequiredBy(pluginFileName);
            }
            else {
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
                    _backgroundWorker.ReportMessage("Getting missing masters for: " + pluginPath, true);
                    GetPluginMissingMasters(pluginPath);
                }
            }
        }

        private string GetDestinationPath(ModOption archiveModOption, Entry entry) {
            if (ArchiveHelpers.ShouldExtract(entry.FileName)) {
                return archiveModOption.GetExtractedEntryPath(entry);
            } else {
                return null;
            }
        }

        private void FindEntries() {
            foreach (ModOption archiveModOption in ArchiveModOptions) {
                archiveModOption.ClearExtractPaths();
                archiveModOption.Archive.Entries.ToList().ForEach(entry => {
                    archiveModOption.SetExtractPath(entry, GetDestinationPath(archiveModOption, entry));
                });
            }
        }

        private void ExtractEntries() {
            foreach (ModOption archiveModOption in ArchiveModOptions) {
                if (!archiveModOption.HasFilesToExtract) continue;
                _backgroundWorker.ReportMessage("Extracting plugins and archives from " + archiveModOption.Name, true);
                archiveModOption.Archive.Extract(entry => {
                    return archiveModOption.GetExtractPath(entry);
                });
            }
        }

        public void ExtractArchives(List<ModOption> archiveModOptions) {
            MissingMasters.Clear();
            _backgroundWorker.RunWorkerAsync(archiveModOptions);
        }
    }
}
