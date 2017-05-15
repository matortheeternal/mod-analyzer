using ModAnalyzer.Analysis.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SevenZipExtractor;
using ModAnalyzer.Analysis.Events;
using ModAnalyzer.Utils;
using ModAnalyzer.Domain.Fomod;

namespace ModAnalyzer.Analysis.Services {
    public class ModAnalyzerService {
        private readonly BackgroundWorker _backgroundWorker;
        private readonly AssetArchiveAnalyzer _assetArchiveAnalyzer;
        private readonly PluginAnalyzer _pluginAnalyzer;
        private ModAnalysis _modAnalysis;
        private List<Tuple<string, ModOption>> EntryOptionMap;

        public event EventHandler<MessageReportedEventArgs> MessageReported;
        public event EventHandler<EventArgs> AnalysisCompleted;

        public ModAnalyzerService() {
            // prepare background worker
            _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            _backgroundWorker.DoWork += BackgroundWork;
            _backgroundWorker.ProgressChanged += BackgroundProgress;

            // prepare analyzers and job queues
            _assetArchiveAnalyzer = new AssetArchiveAnalyzer(_backgroundWorker);
            _pluginAnalyzer = new PluginAnalyzer(_backgroundWorker);
            EntryOptionMap = new List<Tuple<string, ModOption>>();

            // prepare directories
            Directory.CreateDirectory("output");
        }

        private void BackgroundProgress(object sender, ProgressChangedEventArgs e) {
            MessageReportedEventArgs eventArgs = e.UserState as MessageReportedEventArgs;
            MessageReported?.Invoke(this, eventArgs);
        }

        private string GetOutputFilename(List<ModOption> archiveModOptions) {
            ModOption baseOption = archiveModOptions.Find(modOption => modOption.Default);
            if (baseOption == null) {
                baseOption = archiveModOptions.First();
            }
            return baseOption.Name;
        }

        // Background job to analyze a mod
        private void BackgroundWork(object sender, DoWorkEventArgs e) {
            _modAnalysis = new ModAnalysis();
            List<ModOption> archiveModOptions = e.Argument as List<ModOption>;

            // analyze each archive
            try {
                foreach (ModOption archiveModOption in archiveModOptions) {
                    _backgroundWorker.ReportMessage("Analyzing " + archiveModOption.Name + "...", true);
                    _backgroundWorker.ReportMessage("Calculating MD5 Hash.", false);
                    archiveModOption.GetMD5Hash();
                    AnalyzeArchive(archiveModOption);
                    AnalyzeEntries(archiveModOption);
                    _backgroundWorker.ReportMessage(Environment.NewLine, false);
                }

                // save output
                SaveOutputFile(GetOutputFilename(archiveModOptions));
            }
            catch (Exception x) {
                _backgroundWorker.ReportMessage(Environment.NewLine + x.Message, false);
                _backgroundWorker.ReportMessage(x.StackTrace, false);
                _backgroundWorker.ReportMessage("Analysis failed.", true);
            }

            // tell the view model we're done analyzing things
            AnalysisCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void MapPluginDump(string pluginPath, PluginDump dump) {
            foreach (Tuple<string, ModOption> mapping in EntryOptionMap) {
                if (mapping.Item1 == pluginPath)
                    mapping.Item2.AddPluginDump(dump);
            }
        }

        private void MapArchiveAssetPaths(string archivePath, List<string> assets) {
            foreach (Tuple<string, ModOption> mapping in EntryOptionMap)
                if (mapping.Item1 == archivePath)
                    mapping.Item2.AddArchiveAssetPaths(archivePath, assets);
        }

        // performs the enqueued entry analysis jobs
        // TODO: Raise exception if job fails
        private void AnalyzeEntries(ModOption archiveModOption) {
            foreach (string pluginPath in archiveModOption.PluginPaths) {
                PluginDump dump = _pluginAnalyzer.GetPluginDump(pluginPath);
                if (dump == null) throw new Exception("Plugin dump failed.");
                MapPluginDump(pluginPath, dump);
            }
            foreach (string archivePath in archiveModOption.ArchivePaths) {
                List<string> assets = _assetArchiveAnalyzer.GetAssetPaths(archivePath);
                if (assets == null) continue;
                MapArchiveAssetPaths(archivePath, assets);
            }
        }

        private void AnalyzeArchive(ModOption modOption) {
            if (modOption.IsFomodArchive) {
                List<ModOption> fomodOptions = AnalyzeFomodArchive(modOption);
                fomodOptions.ForEach(mo => {
                    mo.MD5Hash = modOption.MD5Hash;
                    mo.Default = mo.Default && modOption.Default;
                });
                _modAnalysis.ModOptions.Add(modOption);
                _modAnalysis.ModOptions.AddRange(fomodOptions);
            }
            else if (modOption.IsBainArchive || modOption.IsFlexArchive) {
                List<ModOption> bainOptions = AnalyzeBainArchive(modOption);
                bainOptions.ForEach(mo => { mo.MD5Hash = modOption.MD5Hash; });
                _modAnalysis.ModOptions.Add(modOption);
                _modAnalysis.ModOptions.AddRange(bainOptions);
            }
            else {
                AnalyzeNormalArchive(modOption);
            }
        }

        private void MapEntryToFomodOption(List<Tuple<FomodFile, ModOption>> map, Entry entry, ModOption archiveModOption) {
            string entryPath = entry.GetPath();
            string fomodBasePath = archiveModOption.BaseInstallerPath;
            if (fomodBasePath.Length > 0) {
                entryPath = entryPath.Replace(fomodBasePath, "");
            }
            foreach (Tuple<FomodFile, ModOption> mapping in map) {
                FomodFile fileNode = mapping.Item1;
                ModOption option = mapping.Item2;

                if (fileNode.MatchesPath(entryPath)) {
                    string mappedPath = fileNode.MappedPath(entryPath);
                    if (mappedPath == "") continue;
                    if (option.Assets.IndexOf(mappedPath) > -1) continue;
                    option.Assets.Add(mappedPath);
                    option.Size += (long)entry.Size;
                    _backgroundWorker.ReportMessage("  " + option.Name + " -> " + mappedPath, false);

                    // enqueue jobs for analyzing archives and plugins
                    MapEntryToOption(entry, option, archiveModOption);
                }
            }
        }

        private void MapEntryToBainOption(List<Tuple<string, ModOption>> map, Entry entry, ModOption archiveModOption) {
            string entryPath = entry.GetPath();
            foreach (Tuple<string, ModOption> mapping in map) {
                string bainPath = mapping.Item1 + @"\";
                ModOption option = mapping.Item2;

                if (entryPath.StartsWith(bainPath)) {
                    string mappedPath = entryPath.Replace(bainPath, "");
                    if (mappedPath == "") continue;
                    option.Assets.Add(mappedPath);
                    option.Size += (long)entry.Size;
                    _backgroundWorker.ReportMessage("  " + option.Name + " -> " + mappedPath, false);

                    // enqueue jobs for analyzing archives and plugins
                    MapEntryToOption(entry, option, archiveModOption);
                }
            }
        }

        private List<ModOption> AnalyzeBainArchive(ModOption archiveModOption) {
            ArchiveFile archive = archiveModOption.Archive;
            bool flex = archiveModOption.IsFlexArchive;
            if (flex) {
                archiveModOption.BaseInstallerPath = archiveModOption.Archive.GetBaseDirectory();
            }
            string bainType = flex ? "FLEX" : "BAIN";
            _backgroundWorker.ReportMessage("Parsing " + bainType + " Options", true);
            List<ModOption> bainOptions = new List<ModOption>();
            List<Tuple<string, ModOption>> bainMap = new List<Tuple<string, ModOption>>();

            // STEP 1. Find BAIN directories and build mod options for them
            foreach (string bainDirectory in archiveModOption.GetInstallerDirectories(!flex)) {
                string bainOptionName = Path.GetFileName(bainDirectory);
                _backgroundWorker.ReportMessage("Found " + bainType + " Option " + bainOptionName, false);
                ModOption bainOption = new ModOption(bainOptionName, false, true);
                bainMap.Add(new Tuple<string, ModOption>(bainDirectory, bainOption));
                bainOptions.Add(bainOption);
            }

            // STEP 2: Map entries to bain options
            _backgroundWorker.ReportMessage(Environment.NewLine + "Mapping assets to " + bainType + " Options", true);
            foreach (Entry entry in archive.FileEntries()) {
                MapEntryToBainOption(bainMap, entry, archiveModOption);
            }

            // Return the mod options we built
            _backgroundWorker.ReportMessage("Done.  " + bainOptions.Count + " " + bainType + " Options found.", true);
            return bainOptions.OrderBy(x => x.Name).ToList();
        }

        private List<ModOption> AnalyzeFomodArchive(ModOption archiveModOption) {
            ArchiveFile archive = archiveModOption.Archive;
            _backgroundWorker.ReportMessage("Parsing FOMOD Options", true);
            List<ModOption> fomodOptions = new List<ModOption>();

            // STEP 1: Find the fomod/ModuleConfig.xml file and extract it
            Entry configEntry = archive.FindArchiveEntry(@"fomod\ModuleConfig.xml");
            _backgroundWorker.ReportMessage("Found FOMOD Config at " + configEntry.GetPath(), false);

            Directory.CreateDirectory(@".\fomod");
            configEntry.ExtractToDirectory(@".\fomod");
            _backgroundWorker.ReportMessage("FOMOD Config Extracted", true);

            // STEP 2: Parse ModuleConfig.xml and determine what the mod options are
            FomodConfig fomodConfig = new FomodConfig(@".\fomod\ModuleConfig.xml");
            fomodOptions = fomodConfig.BuildModOptions(archiveModOption.BaseInstallerPath);

            // STEP 3: Loop through the archive's assets appending them to mod options per mapping
            _backgroundWorker.ReportMessage(Environment.NewLine + "Mapping assets to FOMOD Options", true);
            foreach (Entry entry in archive.FileEntries()) {
                MapEntryToFomodOption(fomodConfig.FileMap, entry, archiveModOption);
            }

            // STEP 4: Delete any options that have no assets or plugins in them
            _backgroundWorker.ReportMessage(Environment.NewLine + "Cleaning up...", true);
            fomodOptions.RemoveAll(ModOption.IsEmpty);

            // Return the mod options we built
            _backgroundWorker.ReportMessage("Done.  " + fomodOptions.Count + " FOMOD Options found.", true);
            return fomodOptions;
        }

        private void AnalyzeNormalArchive(ModOption archiveModOption) {
            ArchiveFile archive = archiveModOption.Archive;
            foreach (Entry modArchiveEntry in archive.FileEntries()) {
                // append entry path to option assets
                string entryPath = modArchiveEntry.GetPath();
                archiveModOption.Assets.Add(entryPath);
                _backgroundWorker.ReportMessage(entryPath, false);

                // enqueue jobs for analyzing archives and plugins
                MapEntryToOption(modArchiveEntry, archiveModOption, archiveModOption);
            }

            _modAnalysis.ModOptions.Add(archiveModOption);
        }

        public void AnalyzeMod(List<ModOption> archiveModOptions) {
            _backgroundWorker.RunWorkerAsync(archiveModOptions);
        }

        private void MapEntryToOption(Entry entry, ModOption option, ModOption archiveModOption) {
            string extractedEntryPath = archiveModOption.GetExtractedEntryPath(entry);
            EntryOptionMap.Add(new Tuple<string, ModOption>(extractedEntryPath, option));
        }

        private void SaveOutputFile(string filePath) {
            string filename = Path.Combine("output", Path.GetFileNameWithoutExtension(filePath));
            _backgroundWorker.ReportMessage("Saving JSON to " + filename + ".json...", true);
            File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(_modAnalysis));
            _backgroundWorker.ReportMessage("All done.  JSON file saved to " + filename + ".json", true);
        }
    }
}