using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ModAnalyzer.Utils;
using Newtonsoft.Json;
using SharpCompress.Archive;

namespace ModAnalyzer.Domain
{
    public static class Consts
    {
        public static readonly string BA2 = ".BA2";
        public static readonly string BSA = ".BSA";
        public static readonly string ESP = ".ESP";
        public static readonly string ESM = ".ESM";

        public static readonly string[] SupportedAssets =
        {
            BA2, BSA
        };

        public static readonly string[] SupportedPlugins =
        {
            ESP, ESM
        };

        public static bool IsSupportedAsset(string ext)
        {
            return SupportedAssets.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsSupportedPlugin(string ext)
        {
            return SupportedPlugins.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsSupportedExtension(string ext)
        {
            return IsSupportedAsset(ext) || IsSupportedPlugin(ext);
        }
    }

    public class ModAnalyzerService
    {
        private readonly AssetArchiveAnalyzer _assetArchiveAnalyzer;
        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
        private readonly List<EntryAnalysisJob> _entryAnalysisJobs = new List<EntryAnalysisJob>();
        private readonly PluginAnalyzer _pluginAnalyzer;
        private ModAnalysis _modAnalysis;

        public ModAnalyzerService()
        {
            PrepareBackgroundWorker();
            // prepare analyzers and job queues
            _assetArchiveAnalyzer = new AssetArchiveAnalyzer(_backgroundWorker);
            _pluginAnalyzer = new PluginAnalyzer(_backgroundWorker);

            // prepare directories
            Directory.CreateDirectory("output");
        }

        private void PrepareBackgroundWorker()
        {
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += BackgroundWork;
            _backgroundWorker.ProgressChanged += BackgroundProgress;
        }

        public event EventHandler<MessageReportedEventArgs> MessageReported;

        private void BackgroundProgress(object sender, ProgressChangedEventArgs e)
        {
            var eventArgs = e.UserState as MessageReportedEventArgs;
            if (MessageReported != null)
                MessageReported(this, eventArgs);
        }

        private static string GetOutputFilename(List<ModOption> archiveModOptions)
        {
            var baseOption = archiveModOptions.Find(modOption => modOption.Default);
            if (baseOption == null)
                baseOption = archiveModOptions.First();
            return baseOption.Name;
        }

        // Background job to analyze a mod
        private void BackgroundWork(object sender, DoWorkEventArgs e)
        {
            _modAnalysis = new ModAnalysis();
            var archiveModOptions = e.Argument as List<ModOption>;
            if (archiveModOptions == null)
                throw new ArgumentNullException(nameof(archiveModOptions));
            // analyze each archive
            try
            {
                foreach (var archiveModOption in archiveModOptions)
                {
                    _backgroundWorker.ReportMessage("Analyzing " + archiveModOption.Name + "...", true);
                    using (var archive = ArchiveFactory.Open(archiveModOption.SourceFilePath))
                    {
                        AnalyzeArchive(archive, archiveModOption);
                        AnalyzeEntries();
                    }
                }
                SaveOutputFile(GetOutputFilename(archiveModOptions)); // save output
            }
            catch (Exception x)
            {
                _backgroundWorker.ReportMessage(x.Message);
                _backgroundWorker.ReportMessage("Analysis failed.", true);
            }
        }

        // performs the enqueued entry analysis jobs
        // TODO: Raise exception if job fails
        private void AnalyzeEntries()
        {
            foreach (var job in _entryAnalysisJobs)
            {
                var ext = job.Entry.GetEntryExtension();
                if (Consts.IsSupportedAsset(ext))
                {
                    var assets = _assetArchiveAnalyzer.GetAssetPaths(job.Entry);
                    if (assets != null)
                        job.AddAssetPaths(assets);
                } else if (Consts.IsSupportedPlugin(ext))
                {
                    var dump = _pluginAnalyzer.GetPluginDump(job.Entry);
                    if (dump != null)
                        job.AddPluginDump(dump);
                    else
                        throw new Exception("Plugin dump failed.");
                }
            }
            _entryAnalysisJobs.Clear(); // clear the entry analysis jobs - they've been performed
        }

        private void AnalyzeArchive(IArchive archive, ModOption modOption)
        {
            var configEntry = archive.Entries.FirstOrDefault(entry => entry.Key.EndsWith("fomod/ModuleConfig.xml", StringComparison.OrdinalIgnoreCase));
            if (configEntry != null)
            {
                var fomodOptions = AnalyzeFomodArchive(archive, configEntry);
                _modAnalysis.ModOptions.AddRange(fomodOptions);
            } else
            {
                modOption.Size = archive.TotalUncompressSize;
                AnalyzeNormalArchive(archive, modOption);
            }
        }

        private void MapEntryToOptionAssets(IEnumerable<Tuple<FomodFile, ModOption>> map, IArchiveEntry entry, string fomodBasePath)
        {
            var entryPath = entry.GetEntryPath();
            if (fomodBasePath.Length > 0)
                entryPath = entryPath.Replace(fomodBasePath, string.Empty);
            foreach (var mapping in map)
            {
                var fileNode = mapping.Item1;
                var option = mapping.Item2;
                if (!fileNode.MatchesPath(entryPath))
                    continue;
                var mappedPath = fileNode.MappedPath(entryPath);
                option.Assets.Add(mappedPath);
                option.Size += entry.Size;
                _backgroundWorker.ReportMessage("  " + option.Name + " -> " + mappedPath);
                EnqueueAnalysisJob(entry, option); // enqueue jobs for analyzing archives and plugins
            }
        }

        private static string GetFomodBasePath(string configEntryPath)
        {
            var configFomodPath = "fomod/ModuleConfig.xml";
            var index = configEntryPath.IndexOf(configFomodPath, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                return configEntryPath.Remove(index, configFomodPath.Length).Replace("/", "\\");
            return string.Empty;
        }

        private IEnumerable<ModOption> AnalyzeFomodArchive(IArchive archive, IArchiveEntry configEntry)
        {
            _backgroundWorker.ReportMessage("Parsing FOMOD Options", true);
            _backgroundWorker.ReportMessage("Found FOMOD Config at " + configEntry.Key);

            // STEP 1: Extract ModuleConfig.xml
            var fomodBasePath = GetFomodBasePath(configEntry.Key);
            Directory.CreateDirectory(@".\fomod");
            configEntry.WriteToDirectory(@".\fomod");
            _backgroundWorker.ReportMessage("FOMOD Config Extracted" + Environment.NewLine, true);

            // STEP 2: Parse ModuleConfig.xml and determine what the mod options are
            var fomodConfig = new FomodConfig(@".\fomod\ModuleConfig.xml");
            var fomodOptions = fomodConfig.BuildModOptions();

            // STEP 3: Loop through the archive's assets appending them to mod options per mapping
            _backgroundWorker.ReportMessage(Environment.NewLine + "Mapping assets to FOMOD Options", true);
            foreach (var entry in archive.Entries)
                MapEntryToOptionAssets(fomodConfig.FileMap, entry, fomodBasePath);

            // STEP 4: Delete any options that have no assets or plugins in them
            _backgroundWorker.ReportMessage(Environment.NewLine + "Cleaning up...", true);
            fomodOptions.RemoveAll(ModOption.IsEmpty);

            // Return the mod options we built
            _backgroundWorker.ReportMessage("Done.  " + fomodOptions.Count + " FOMOD Options found.", true);
            return fomodOptions;
        }

        private void AnalyzeNormalArchive(IArchive archive, ModOption option)
        {
            foreach (var modArchiveEntry in archive.Entries)
            {
                if (modArchiveEntry.IsDirectory)
                    continue;
                // append entry path to option assets
                var entryPath = modArchiveEntry.GetEntryPath();
                option.Assets.Add(entryPath);
                _backgroundWorker.ReportMessage(entryPath);
                // enqueue jobs for analyzing archives and plugins
                EnqueueAnalysisJob(modArchiveEntry, option);
            }
            _modAnalysis.ModOptions.Add(option);
        }

        public void AnalyzeMod(List<ModOption> archiveModOptions)
        {
            _backgroundWorker.RunWorkerAsync(archiveModOptions);
        }

        private void EnqueueAnalysisJob(IArchiveEntry entry, ModOption option)
        {
            if (!Consts.IsSupportedExtension(entry.GetEntryExtension()))
                return;
            var foundJob = _entryAnalysisJobs.Find(job => job.Entry.Equals(entry));
            if (foundJob != null)
                foundJob.Options.Add(option);
            else
            {
                var job = new EntryAnalysisJob(entry);
                job.Options.Add(option);
                _entryAnalysisJobs.Add(job);
            }
        }

        private void SaveOutputFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            var filename = Path.Combine("output", Path.GetFileNameWithoutExtension(filePath));
            _backgroundWorker.ReportMessage("Saving JSON to " + filename + ".json...", true);
            File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(_modAnalysis));
            _backgroundWorker.ReportMessage("All done.  JSON file saved to " + filename + ".json", true);
        }
    }
}
