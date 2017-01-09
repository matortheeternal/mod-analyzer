using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// TODO
    /// </summary>
    public class FomodConfig {
        public string GroupType { get; set; }
        public List<FomodPlugin> Plugins { get; set; }
        public List<FomodPattern> Patterns { get; set; }
        public List<FomodFile> BaseFiles { get; set; }
        public List<Tuple<FomodFile, ModOption>> FileMap { get; set; }

        public FomodConfig(string xmlPath) {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlText = File.ReadAllText(@".\fomod\ModuleConfig.xml");
            xmlDoc.LoadXml(xmlText);
            Plugins = FomodPlugin.FromDocument(xmlDoc);
            Patterns = FomodPattern.FromDocument(xmlDoc);
            FileMap = new List<Tuple<FomodFile, ModOption>>();
            GetBaseFiles(xmlDoc);
            MapPluginPatterns();
        }

        private void GetBaseFiles(XmlDocument doc) {
            XmlNodeList baseNodes = doc.GetElementsByTagName("requiredInstallFiles");
            if (baseNodes.Count > 0) {
                BaseFiles = FomodFile.FromNodes(baseNodes[0].ChildNodes);
            }
        }

        private List<FomodPattern> GetMatchingPatterns(FomodFlag flag) {
            List<FomodPattern> results = new List<FomodPattern>();
            foreach (FomodPattern pattern in Patterns) {
                if (pattern.Dependencies != null) {
                    foreach (FomodFlagDependency dependency in pattern.Dependencies) {
                        if (flag.Matches(dependency)) {
                            results.Add(pattern);
                        }
                    }
                }
            }
            return results;
        }

        private void MapPluginPatterns() {
            foreach (FomodPlugin plugin in Plugins) {
                if (plugin.Flags != null) {
                    foreach (FomodFlag flag in plugin.Flags) {
                        foreach (FomodPattern pattern in GetMatchingPatterns(flag)) {
                            plugin.Files.AddRange(pattern.Files);
                        }
                    }
                }
            }
        }

        private void AddFileMappings(ModOption option, List<FomodFile> files) {
            foreach (FomodFile file in files) {
                Tuple<FomodFile, ModOption> mapping = new Tuple<FomodFile, ModOption>(file, option);
                if (FileMap.IndexOf(mapping) == -1) {
                    FileMap.Add(mapping);
                }
            }
        }

        public List<ModOption> BuildModOptions() {
            List<ModOption> options = new List<ModOption>();
            if (BaseFiles != null) {
                ModOption baseOption = new ModOption("Base FOMOD Files", true, true);
                AddFileMappings(baseOption, BaseFiles);
                options.Add(baseOption);
            }
            foreach (FomodPlugin plugin in Plugins) {
                ModOption option = new ModOption(plugin.Name, plugin.IsDefault(), true);
                if (plugin.Files != null) {
                    AddFileMappings(option, plugin.Files);
                }
                options.Add(option);
            }
            return options;
        }
    }
}