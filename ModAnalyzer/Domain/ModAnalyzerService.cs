using ModAnalyzer.Utils;
using Newtonsoft.Json;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ModAnalyzer.Domain {
    public class ModAnalyzerService {
        private readonly BackgroundWorker _backgroundWorker;
        private readonly AssetArchiveAnalyzer _assetArchiveAnalyzer;
        private readonly PluginAnalyzer _pluginAnalyzer;
        private ModAnalysis _modAnalysis;
        private List<EntryAnalysisJob> entryAnalysisJobs;
        private readonly string[] entryJobExtensions = { ".BA2", ".BSA", ".ESP", ".ESM" };

        public event EventHandler<MessageReportedEventArgs> MessageReported;

        public ModAnalyzerService() {
            // prepare background worker
            _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            _backgroundWorker.DoWork += BackgroundWork;
            _backgroundWorker.ProgressChanged += BackgroundProgress;

            // prepare analyzers and job queues
            _assetArchiveAnalyzer = new AssetArchiveAnalyzer(_backgroundWorker);
            _pluginAnalyzer = new PluginAnalyzer(_backgroundWorker);
            entryAnalysisJobs = new List<EntryAnalysisJob>();

            // prepare directories
            Directory.CreateDirectory("output");
        }

        private void BackgroundProgress(object sender, ProgressChangedEventArgs e) {
            MessageReportedEventArgs eventArgs = e.UserState as MessageReportedEventArgs;

            MessageReported?.Invoke(this, eventArgs);
        }

        // Background job to analyze a mod
        private void BackgroundWork(object sender, DoWorkEventArgs e) {
            _modAnalysis = new ModAnalysis();
            List<string> archivePaths = e.Argument as List<string>;

            // analyze each archive
            foreach(string archivePath in archivePaths) {
                _backgroundWorker.ReportMessage("Analyzing " + Path.GetFileName(archivePath) + "...", true);

                using (IArchive archive = ArchiveFactory.Open(archivePath)) {
                    AnalyzeArchive(archive, archivePath);
                    AnalyzeEntries();
                }
            }

            // if we have only one mod option, it must be default
            if (_modAnalysis.ModOptions.Count == 1) {
                _modAnalysis.ModOptions[0].Default = true;
            }

            // TODO: This should get the name of the base mod option or something
            string filename = archivePaths[0];
            SaveOutputFile(filename);
        }

        private IArchiveEntry FindArchiveEntry(IArchive archive, string path) {
            foreach (IArchiveEntry entry in archive.Entries) {
                if (path.Equals(entry.Key, StringComparison.CurrentCultureIgnoreCase)) {
                    return entry;
                }
            }
            return null;
        }

        // performs the enqueued entry analysis jobs
        // TODO: Raise exception if job fails
        private void AnalyzeEntries() {
            foreach (EntryAnalysisJob job in entryAnalysisJobs) {
                string ext = job.Entry.GetEntryExtension();
                switch (ext) {
                    case ".BSA":
                    case ".BA2":
                        List<String> assets = _assetArchiveAnalyzer.GetAssetPaths(job.Entry);
                        if (assets != null) {
                            job.AddAssetPaths(assets);
                        }
                        break;
                    case ".ESP":
                    case ".ESM":
                        PluginDump dump = _pluginAnalyzer.GetPluginDump(job.Entry);
                        if (dump != null) {
                            job.AddPluginDump(dump);
                        }
                        break;
                }
            }

            // clear the entry analysis jobs - they've been performed
            entryAnalysisJobs.Clear();
        }

        private void AnalyzeArchive(IArchive archive, string archivePath) {
            if (IsFomodArchive(archive)) {
                List<ModOption> fomodOptions = AnalyzeFomodArchive(archive);
                _modAnalysis.ModOptions.AddRange(fomodOptions);
            }
            else {
                ModOption option = AnalyzeNormalArchive(archive);
                option.Name = Path.GetFileName(archivePath);
                option.Size = archive.TotalUncompressSize;
                _modAnalysis.ModOptions.Add(option);
            }
        }

        private bool IsFomodArchive(IArchive archive) {
            return FindArchiveEntry(archive, "fomod/ModuleConfig.xml") != null;
        }

        private void MapEntryToOptionAssets(List<Tuple<FomodFile, ModOption>> map, IArchiveEntry entry) {
            string entryPath = entry.GetEntryPath();
            foreach (Tuple<FomodFile, ModOption> mapping in map) {
                FomodFile fileNode = mapping.Item1;
                ModOption option = mapping.Item2;

                if (fileNode.MatchesPath(entryPath)) {
                    string mappedPath = fileNode.MappedPath(entryPath);
                    option.Assets.Add(mappedPath);
                    option.Size += entry.Size;
                    _backgroundWorker.ReportMessage("  " + option.Name + " -> " + mappedPath, false);

                    // enqueue jobs for analyzing archives and plugins
                    EnqueueAnalysisJob(entry, option);
                }
            }
        }

        private List<ModOption> AnalyzeFomodArchive(IArchive archive) {
            _backgroundWorker.ReportMessage("Parsing FOMOD Options", true);
            List<ModOption> fomodOptions = new List<ModOption>();

            // STEP 1: Find the fomod/ModuleConfig.xml file and extract it
            IArchiveEntry configEntry = FindArchiveEntry(archive, "fomod/ModuleConfig.xml");
            Directory.CreateDirectory(@".\fomod");
            configEntry.WriteToDirectory(@".\fomod", ExtractOptions.Overwrite);
            _backgroundWorker.ReportMessage("FOMOD Config Extracted" + Environment.NewLine, true);

            // STEP 2: Parse ModuleConfig.xml and determine what the mod options are
            FomodConfig fomodConfig = new FomodConfig(@".\fomod\ModuleConfig.xml");
            fomodOptions = fomodConfig.BuildModOptions();

            // STEP 3: Loop through the archive's assets appending them to mod options per mapping
            _backgroundWorker.ReportMessage(Environment.NewLine + "Mapping assets to FOMOD Options", true);
            foreach (IArchiveEntry entry in archive.Entries) {
                MapEntryToOptionAssets(fomodConfig.FileMap, entry);
            }

            // STEP 4: Delete any options that have no assets or plugins in them
            _backgroundWorker.ReportMessage(Environment.NewLine + "Cleaning up...", true);
            fomodOptions.RemoveAll(ModOption.IsEmpty);

            // Return the mod options we built
            _backgroundWorker.ReportMessage("Done.  " + fomodOptions.Count + " FOMOD Options found.", true);
            return fomodOptions;
        }

        private ModOption AnalyzeNormalArchive(IArchive archive) {
            ModOption option = new ModOption();

            foreach (IArchiveEntry modArchiveEntry in archive.Entries) {
                if (modArchiveEntry.IsDirectory)
                    continue;

                // append entry path to option assets
                string entryPath = modArchiveEntry.GetEntryPath();
                option.Assets.Add(entryPath);
                _backgroundWorker.ReportMessage(entryPath, false);

                // enqueue jobs for analyzing archives and plugins
                EnqueueAnalysisJob(modArchiveEntry, option);
            }

            return option;
        }

        public void AnalyzeMod(List<ModOption> modArchivePaths) {
            _backgroundWorker.RunWorkerAsync(modArchivePaths);
        }

        private void EnqueueAnalysisJob(IArchiveEntry entry, ModOption option) {
            if (entryJobExtensions.Contains(entry.GetEntryExtension())) {
                EntryAnalysisJob foundJob = entryAnalysisJobs.Find(job => job.Entry.Equals(entry));
                if (foundJob != null) {
                    foundJob.AddOption(option);
                }
                else {
                    entryAnalysisJobs.Add(new EntryAnalysisJob(entry, option));
                }
            }
        }

        private void SaveOutputFile(string filePath) {
            string filename = Path.Combine("output", Path.GetFileNameWithoutExtension(filePath));

            _backgroundWorker.ReportMessage("Saving JSON to " + filename + ".json...", true);
            File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(_modAnalysis));
            _backgroundWorker.ReportMessage("All done.  JSON file saved to " + filename + ".json", true);
        }
    }
}
