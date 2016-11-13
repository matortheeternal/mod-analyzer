using System;
using System.Collections.Generic;
using System.Xml;

namespace ModAnalyzer.Domain
{
    /// <summary>
    ///     TODO
    /// </summary>
    public class FomodConfig
    {
        public FomodConfig(string xmlPath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            Plugins = FomodPlugin.FromDocument(xmlDoc);
            Patterns = FomodPattern.FromDocument(xmlDoc);
            FileMap = new List<Tuple<FomodFile, ModOption>>();
            GetBaseFiles(xmlDoc);
            MapPluginPatterns();
        }

        public string GroupType { get; set; }
        public List<FomodPlugin> Plugins { get; set; }
        public List<FomodPattern> Patterns { get; set; }
        public List<FomodFile> BaseFiles { get; set; }
        public List<Tuple<FomodFile, ModOption>> FileMap { get; set; }

        private void GetBaseFiles(XmlDocument doc)
        {
            var baseNodes = doc.GetElementsByTagName("requiredInstallFiles");
            if (baseNodes.Count > 0)
                BaseFiles = FomodFile.FromNodes(baseNodes[0].ChildNodes);
        }

        private List<FomodPattern> GetMatchingPatterns(FomodFlag flag)
        {
            var results = new List<FomodPattern>();
            foreach (var pattern in Patterns)
                if (pattern.Dependencies != null)
                    foreach (var dependency in pattern.Dependencies)
                        if (flag.Matches(dependency))
                            results.Add(pattern);
            return results;
        }

        private void MapPluginPatterns()
        {
            foreach (var plugin in Plugins)
                if (plugin.Flags != null)
                    foreach (var flag in plugin.Flags)
                        foreach (var pattern in GetMatchingPatterns(flag))
                            plugin.Files.AddRange(pattern.Files);
        }

        private void AddFileMappings(ModOption option, List<FomodFile> files)
        {
            foreach (var file in files)
                FileMap.Add(new Tuple<FomodFile, ModOption>(file, option));
        }

        public List<ModOption> BuildModOptions()
        {
            var options = new List<ModOption>();
            if (BaseFiles != null)
            {
                var baseOption = new ModOption("Base FOMOD Files", true, true);
                AddFileMappings(baseOption, BaseFiles);
                options.Add(baseOption);
            }
            foreach (var plugin in Plugins)
            {
                var option = new ModOption(plugin.Name, plugin.IsDefault(), true);
                if (plugin.Files != null)
                    AddFileMappings(option, plugin.Files);
                options.Add(option);
            }
            return options;
        }
    }
}
