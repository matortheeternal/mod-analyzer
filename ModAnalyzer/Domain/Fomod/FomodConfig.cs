using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ModAnalyzer.Analysis.Models;

namespace ModAnalyzer.Domain.Fomod {
    public class FomodConfig {
        public string GroupType { get; set; }
        public List<FomodPlugin> Plugins { get; set; }
        public List<FomodPattern> Patterns { get; set; }
        public List<FomodFile> BaseFiles { get; set; }
        public List<Tuple<FomodFile, ModOption>> FileMap { get; set; }

        public FomodConfig(string xmlPath, bool skipPatterns = false) {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlText = File.ReadAllText(@".\fomod\ModuleConfig.xml");
            xmlDoc.LoadXml(xmlText);
            Plugins = FomodPlugin.FromDocument(xmlDoc);
            Patterns = FomodPattern.FromDocument(xmlDoc);
            FileMap = new List<Tuple<FomodFile, ModOption>>();
            GetBaseFiles(xmlDoc);
            if (!skipPatterns) MapPluginPatterns();
        }

        private void GetBaseFiles(XmlDocument doc) {
            XmlNodeList baseNodes = doc.GetElementsByTagName("requiredInstallFiles");
            if (baseNodes.Count > 0) {
                BaseFiles = FomodFile.FromNodes(baseNodes[0].ChildNodes);
            }
        }

        private List<FomodPattern> GetSatisfiedPatterns(Dictionary<string, string> flags) {
            List<FomodPattern> results = new List<FomodPattern>();
            foreach (FomodPattern pattern in Patterns) {
                if (pattern.Dependencies != null) {
                    foreach (FomodFlagDependency dependency in pattern.Dependencies) {
                        if (flags.ContainsKey(dependency.Flag) && flags[dependency.Flag].Equals(dependency.Value)) {
                            results.Add(pattern);
                        }
                    }
                }
            }
            return results;
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
            option.FomodFiles.AddRange(files);
            foreach (FomodFile file in files) {
                Tuple<FomodFile, ModOption> mapping = new Tuple<FomodFile, ModOption>(file, option);
                if (FileMap.IndexOf(mapping) == -1) {
                    FileMap.Add(mapping);
                }
            }
        }

        public List<FomodFile> GetFilesToInstall(List<string> ModOptionNames, bool ignoreMissing = false) {
            List<FomodFile> filesToInstall = new List<FomodFile>();
            Dictionary<string, string> flags = new Dictionary<string, string>();
            if (ModOptionNames.Contains("Base FOMOD Files")) {
                filesToInstall.AddRange(BaseFiles);
            }
            foreach (string name in ModOptionNames) {
                FomodPlugin foundPlugin = Plugins.Find(p => p.Name.Equals(name));
                if (foundPlugin != null) {
                    filesToInstall.AddRange(foundPlugin.Files);
                    foundPlugin.Flags.ForEach(f => flags[f.Name] = f.Value);
                } else {
                    if (!ignoreMissing) throw new Exception("Mod Option " + name + " not found.");
                }
            }
            foreach (FomodPattern pattern in GetSatisfiedPatterns(flags)) {
                filesToInstall.AddRange(pattern.Files);
            }
            return filesToInstall;
        }

        private List<string> GetChangedFlags(List<ModOption> options) {
            List<string> changedFlags = new List<string>();
            var dependencies = new Dictionary<String, String>();
            foreach (ModOption option in options) {
                if (option.Flags == null) continue;
                foreach (KeyValuePair<string, string> flag in option.Flags) {
                    if (dependencies.ContainsKey(flag.Key)) {
                        if (!dependencies[flag.Key].Equals(flag.Value) && !changedFlags.Contains(flag.Key))
                            changedFlags.Add(flag.Key);
                    } else {
                        dependencies.Add(flag.Key, flag.Value);
                    }
                }
            }
            return changedFlags;
        }

        private string DisplayFlagValue(string value) {
            if (value.Equals("Off", StringComparison.CurrentCultureIgnoreCase) ||
                string.IsNullOrEmpty(value) || value.Equals("0")) return "Off";
            return "On";
        }

        private void SpecifyModOptionNames(List<ModOption> options) {
            List<string> changedFlags = GetChangedFlags(options);
            if (changedFlags.Count == 0) return;
            foreach (ModOption option in options) {
                if (option.Flags == null || option.Flags.Count == 0) continue;
                string[] flags = changedFlags
                    .FindAll(f => option.Flags.ContainsKey(f))
                    .Select(f =>f + "=" + DisplayFlagValue(option.Flags[f])).ToArray();
                if (flags.Length == 0) return;
                option.Name = string.Format("[{0}] {1}", string.Join(", ", flags), option.Name);
            }
        }

        public List<ModOption> BuildModOptions(string fomodBasePath) {
            List<ModOption> options = new List<ModOption>();
            if (BaseFiles != null) {
                ModOption baseOption = new ModOption("Base FOMOD Files", true, true);
                AddFileMappings(baseOption, BaseFiles);
                options.Add(baseOption);
            }
            foreach (FomodPlugin plugin in Plugins) {
                ModOption option = new ModOption(plugin);
                if (plugin.Files != null)
                    AddFileMappings(option, plugin.Files);
                if (!options.Exists(o => o.Matches(option)))
                    options.Add(option);
            }
            foreach (ModOption option in options) {
                List<ModOption> matchingOpts = options.FindAll(o => o.Name.Equals(option.Name));
                if (matchingOpts.Count > 1) SpecifyModOptionNames(matchingOpts);
            }
            return options;
        }
    }
}