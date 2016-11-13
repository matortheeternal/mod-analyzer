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
    public class ModAnalyzerService
    {
        private readonly AssetArchiveAnalyzer _assetArchiveAnalyzer;
        private readonly BackgroundWorker _backgroundWorker;
        private readonly List<EntryAnalysisJob> _entryAnalysisJobs;

        private readonly string[] _entryJobExtensions =
        {
            ".BA2", ".BSA", ".ESP", ".ESM"
        };

        private readonly PluginAnalyzer _pluginAnalyzer;
        private ModAnalysis _modAnalysis;

        public ModAnalyzerService()
        {
            // prepare background worker
            _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            _backgroundWorker.DoWork += BackgroundWork;
            _backgroundWorker.ProgressChanged += BackgroundProgress;

            // prepare analyzers and job queues
            _assetArchiveAnalyzer = new AssetArchiveAnalyzer(_backgroundWorker);
            _pluginAnalyzer = new PluginAnalyzer(_backgroundWorker);
            _entryAnalysisJobs = new List<EntryAnalysisJob>();

            // prepare directories
            Directory.CreateDirectory("output");
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
                _backgroundWorker.ReportMessage(x.Message, false);
                _backgroundWorker.ReportMessage("Analysis failed.", true);
            }
        }

        private IArchiveEntry FindArchiveEntry(IArchive archive, string path)
        {
            foreach (var entry in archive.Entries)
                if (entry.Key.EndsWith(path, StringComparison.CurrentCultureIgnoreCase))
                    return entry;
            return null;
        }

        // performs the enqueued entry analysis jobs
        // TODO: Raise exception if job fails
        private void AnalyzeEntries()
        {
            foreach (var job in _entryAnalysisJobs)
            {
                var ext = job.Entry.GetEntryExtension();
                switch (ext)
                {
                    case ".BSA":
                    case ".BA2":
                        var assets = _assetArchiveAnalyzer.GetAssetPaths(job.Entry);
                        if (assets != null)
                            job.AddAssetPaths(assets);
                        break;
                    case ".ESP":
                    case ".ESM":
                        var dump = _pluginAnalyzer.GetPluginDump(job.Entry);
                        if (dump != null)
                            job.AddPluginDump(dump);
                        else
                            throw new Exception("Plugin dump failed.");
                        break;
                }
            }
            _entryAnalysisJobs.Clear(); // clear the entry analysis jobs - they've been performed
        }

        private void AnalyzeArchive(IArchive archive, ModOption modOption)
        {
            if (IsFomodArchive(archive))
            {
                var fomodOptions = AnalyzeFomodArchive(archive);
                _modAnalysis.ModOptions.AddRange(fomodOptions);
            } else
            {
                modOption.Size = archive.TotalUncompressSize;
                AnalyzeNormalArchive(archive, modOption);
            }
        }

        private bool IsFomodArchive(IArchive archive)
        {
            return FindArchiveEntry(archive, "fomod/ModuleConfig.xml") != null;
        }

        private void MapEntryToOptionAssets(IEnumerable<Tuple<FomodFile, ModOption>> map, IArchiveEntry entry, string fomodBasePath)
        {
            var entryPath = entry.GetEntryPath();
            if (fomodBasePath.Length > 0)
                entryPath = entryPath.Replace(fomodBasePath, "");
            foreach (var mapping in map)
            {
                var fileNode = mapping.Item1;
                var option = mapping.Item2;

                if (fileNode.MatchesPath(entryPath))
                {
                    var mappedPath = fileNode.MappedPath(entryPath);
                    option.Assets.Add(mappedPath);
                    option.Size += entry.Size;
                    _backgroundWorker.ReportMessage("  " + option.Name + " -> " + mappedPath, false);

                    // enqueue jobs for analyzing archives and plugins
                    EnqueueAnalysisJob(entry, option);
                }
            }
        }

        private string GetFomodBasePath(string configEntryPath)
        {
            var configFomodPath = "fomod/ModuleConfig.xml";
            var index = configEntryPath.IndexOf(configFomodPath, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                return configEntryPath.Remove(index, configFomodPath.Length).Replace("/", "\\");
            return "";
        }

        private List<ModOption> AnalyzeFomodArchive(IArchive archive)
        {
            _backgroundWorker.ReportMessage("Parsing FOMOD Options", true);

            // STEP 1: Find the fomod/ModuleConfig.xml file and extract it
            var configEntry = FindArchiveEntry(archive, "fomod/ModuleConfig.xml");
            _backgroundWorker.ReportMessage("Found FOMOD Config at " + configEntry.Key, false);
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
                _backgroundWorker.ReportMessage(entryPath, false);

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
            if (!_entryJobExtensions.Contains(entry.GetEntryExtension()))
                return;
            var foundJob = _entryAnalysisJobs.Find(job => job.Entry.Equals(entry));
            if (foundJob != null)
                foundJob.AddOption(option);
            else
                _entryAnalysisJobs.Add(new EntryAnalysisJob(entry, option));
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
