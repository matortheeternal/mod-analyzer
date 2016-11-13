using System;
using System.Collections.Generic;
using System.Linq;
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
            var baseNodes = xmlDoc.GetElementsByTagName("requiredInstallFiles");
            if (baseNodes.Count > 0)
                BaseFiles = FomodFile.FromNodes(baseNodes[0].ChildNodes);
            MapPluginPatterns();
        }

        public List<FomodPlugin> Plugins { get; }
        public List<FomodPattern> Patterns { get; }
        public List<FomodFile> BaseFiles { get; }
        public List<Tuple<FomodFile, ModOption>> FileMap { get; } = new List<Tuple<FomodFile, ModOption>>();

        private IEnumerable<FomodPattern> GetMatchingPatterns(FomodFlag flag)
        {
            return Patterns.Where(x => x.Dependencies != null).SelectMany(pattern => pattern.Dependencies.Where(flag.Matches), (pattern, dependency) => pattern);
        }

        private void MapPluginPatterns()
        {
            foreach (var plugin in Plugins.Where(x => x.Flags != null))
                foreach (var flag in plugin.Flags)
                    foreach (var pattern in GetMatchingPatterns(flag))
                        plugin.Files.AddRange(pattern.Files);
        }

        private void AddFileMappings(ModOption option, IEnumerable<FomodFile> files)
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
                var option = new ModOption(plugin.Name, plugin.IsDefault, true);
                if (plugin.Files != null)
                    AddFileMappings(option, plugin.Files);
                options.Add(option);
            }
            return options;
        }
    }
}
