using ModAnalyzer.Utils;
using Newtonsoft.Json;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Xml;
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
        private ModAnalysis _modAnalysis;

        public event EventHandler<MessageReportedEventArgs> MessageReported;
        
        public ModAnalyzerService()
        {
            _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;

            _assetArchiveAnalyzer = new AssetArchiveAnalyzer(_backgroundWorker);
            _pluginAnalyzer = new PluginAnalyzer(_backgroundWorker);

            Directory.CreateDirectory("output");
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e) 
        {
            _modAnalysis = new ModAnalysis();
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
                        option.Name = Path.GetFileName(modArchivePath);
                        option.Size = archive.TotalUncompressSize;
                        _modAnalysis.ModOptions.Add(option);
                    }
                }
            }

            // if we have only one mod option, it must be default
            if (_modAnalysis.ModOptions.Count == 1) {
                _modAnalysis.ModOptions[0].Default = true;
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

        private IArchiveEntry FindArchiveEntry(IArchive archive, string path) 
        {
            foreach (IArchiveEntry modArchiveEntry in archive.Entries) 
            {
                if (path.Equals(modArchiveEntry.Key)) 
                {
                    return modArchiveEntry;
                }
            }
            return null;
        }

        private bool IsFomodArchive(IArchive archive) 
        {
            return FindArchiveEntry(archive, "fomod/ModuleConfig.xml") != null;
        }

        private void MapEntryToOptionAssets(List<Tuple<FomodFileNode, ModOption>> map, IArchiveEntry entry) 
        {
            string entryPath = entry.GetEntryPath();
            foreach (Tuple<FomodFileNode, ModOption> mapping in map) 
            {
                FomodFileNode fileNode = mapping.Item1;
                ModOption option = mapping.Item2;

                if (fileNode.MatchesPath(entryPath))
                {
                    string mappedPath = fileNode.MappedPath(entryPath);
                    option.Assets.Add(mappedPath);
                    option.Size += entry.Size;
                    ReportProgress("  " + option.Name + " -> " + mappedPath);

                    // NOTE: This will analyze the same BSA/plugin multiple times if it appears in multiple fomod options
                    // TODO: Fix that.
                    AnalyzeModArchiveEntry(entry, option);
                }
            }
        }

        private bool HasDependencyNode(XmlNode patternNode, string flagName) 
        {
            XmlNode dependencies = patternNode["dependencies"];
            foreach (XmlNode dependency in dependencies.ChildNodes) 
            {
                if (dependency.Attributes["flag"].Value.Equals(flagName)) 
                {
                    return true;
                }
            }
            return false;
        }

        private List<FomodFileNode> GetFomodFileNodes(XmlNode node) 
        {
            List<FomodFileNode> results = new List<FomodFileNode>();
            foreach (XmlNode childNode in node.ChildNodes) 
            {
                FomodFileNode fileNode = new FomodFileNode(childNode);
                results.Add(fileNode);
                ReportProgress("  + '" + fileNode.Source + "' -> '" + fileNode.Destination + "'");
            }
            return results;
        }

        private List<FomodFileNode> GetMatchingFileNodes(XmlDocument xmlDoc, XmlNode flagNode) 
        {
            List<FomodFileNode> results = new List<FomodFileNode>();
            XmlNodeList patternElements = xmlDoc.GetElementsByTagName("pattern");
            foreach (XmlNode pattern in patternElements) 
            {
                if (HasDependencyNode(pattern, flagNode.Attributes["name"].Value))
                {
                    results.AddRange(GetFomodFileNodes(pattern));
                }
            }

            return results;
        }

        private List<ModOption> AnalyzeFomodArchive(IArchive archive) 
        {
            ReportProgress("Parsing FOMOD Options");
            List<ModOption> fomodOptions = new List<ModOption>();
            List<Tuple<FomodFileNode, ModOption>> fomodFileMap = new List<Tuple<FomodFileNode, ModOption>>();

            // STEP 1: Find the fomod/ModuleConfig.xml file and extract it
            IArchiveEntry configEntry = FindArchiveEntry(archive, "fomod/ModuleConfig.xml");
            Directory.CreateDirectory(@".\fomod");
            configEntry.WriteToDirectory(@".\fomod", ExtractOptions.Overwrite);
            ReportProgress("FOMOD Config Extracted" + Environment.NewLine);

            // STEP 2: Parse info.xml and determine what the mod options are
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@".\fomod\ModuleConfig.xml");

            // STEP 3: Create base fomod option
            XmlNodeList baseFiles = xmlDoc.GetElementsByTagName("requiredInstallFiles");
            if (baseFiles.Count > 0) 
            {
                ModOption option = new ModOption();
                option.Name = "FOMOD Base Files";
                option.Size = 0;
                option.IsFomodOption = true;
                option.Default = true;
                fomodOptions.Add(option);
                ReportProgress("Found FOMOD Option: " + option.Name);

                foreach (XmlNode childNode in baseFiles[0].ChildNodes) 
                {
                    FomodFileNode fileNode = new FomodFileNode(childNode);
                    fomodFileMap.Add(new Tuple<FomodFileNode, ModOption>(fileNode, option));
                    ReportProgress("  + '" + fileNode.Source + "' -> '" + fileNode.Destination + "'");
                }
            }

            // loop through the plugin elements
            XmlNodeList pluginElements = xmlDoc.GetElementsByTagName("plugin");
            foreach (XmlNode node in pluginElements) 
            {
                ModOption option = new ModOption();
                option.Name = node.Attributes["name"].Value;
                option.Size = 0;
                option.IsFomodOption = true;
                fomodOptions.Add(option);
                ReportProgress("Found FOMOD Option: " + option.Name);

                XmlNode files = node["files"];
                XmlNode flags = node["conditionFlags"];
                // loop through the file/folder nodes to create mapping
                if (files != null) 
                {
                    fomodFileMap.AddRange(GetFomodFileNodes(files));
                }
                // else loop through and resolve flags in conditionFlags node to create mapping
                else if (flags != null) 
                {
                    foreach (XmlNode childNode in flags.ChildNodes) {
                        List<FomodFileNode> matchingFileNodes = GetMatchingFileNodes(xmlDoc, childNode);
                        foreach (FomodFileNode fileNode in matchingFileNodes) 
                        {
                            fomodFileMap.Add(new Tuple<FomodFileNode, ModOption>(fileNode, option));
                            ReportProgress("  + '" + fileNode.Source + "' -> '" + fileNode.Destination + "'");
                        }
                    }
                }
            }

            // STEP 4: Loop through the archive's assets appending them to mod options per mapping
            ReportProgress(Environment.NewLine + "Mapping assets to FOMOD Options");
            foreach (IArchiveEntry entry in archive.Entries) 
            {
                MapEntryToOptionAssets(fomodFileMap, entry);
            }

            // STEP 5: Delete any options that have no assets or plugins in them
            ReportProgress(Environment.NewLine + "Cleaning up...");
            fomodOptions.RemoveAll(ModOption.IsEmpty);

            // Return the mod options we built
            ReportProgress("Done.  " + fomodOptions.Count + " FOMOD Options found.");
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
                ReportProgress(entryPath);

                // handle BSAs and plugins
                AnalyzeModArchiveEntry(modArchiveEntry, option);
            }

            return option;
        }

        public void AnalyzeMod(List<string> modArchivePaths)
        {
            _backgroundWorker.RunWorkerAsync(modArchivePaths);
        }

        private void AnalyzeModArchiveEntry(IArchiveEntry entry, ModOption option)
        {
            switch (entry.GetEntryExtension())
            {
                case ".BA2":
                case ".BSA":
                    List<String> assets = _assetArchiveAnalyzer.GetAssets(entry);
                    option.Assets.AddRange(assets);
                    break;
                case ".ESP":
                case ".ESM":
                    PluginDump pluginDump = _pluginAnalyzer.GetPluginDump(entry);
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
