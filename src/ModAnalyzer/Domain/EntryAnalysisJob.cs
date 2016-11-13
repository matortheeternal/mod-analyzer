using System.Collections.Generic;
using SharpCompress.Archive;

namespace ModAnalyzer.Domain
{
    /// <summary>
    ///     A job to analyze a BSA, BA2, ESP, or ESM in a mod.
    /// </summary>
    public class EntryAnalysisJob
    {
        public EntryAnalysisJob(IArchiveEntry entry)
        {
            Entry = entry;
        }

        public IArchiveEntry Entry { get; }
        public List<ModOption> Options { get; } = new List<ModOption>();

        public void AddAssetPaths(List<string> assetPaths)
        {
            foreach (var option in Options)
                option.Assets.AddRange(assetPaths);
        }

        public void AddPluginDump(PluginDump dump)
        {
            foreach (var option in Options)
                option.Plugins.Add(dump);
        }
    }
}
