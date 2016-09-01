using SharpCompress.Archive;
using System;
using System.Collections.Generic;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// A job to analyze a BSA, BA2, ESP, or ESM in a mod.
    /// </summary>
    public class EntryAnalysisJob {
        public IArchiveEntry Entry { get; set; }
        public List<ModOption> Options { get; set; }

        public EntryAnalysisJob(IArchiveEntry Entry, ModOption Option) {
            this.Entry = Entry;
            Options = new List<ModOption>();
            Options.Add(Option);
        }

        public void AddOption(ModOption Option) {
            Options.Add(Option);
        }

        public void AddAssetPaths(List<String> assetPaths) {
            foreach (ModOption option in Options) {
                option.Assets.AddRange(assetPaths);
            }
        }

        public void AddPluginDump(PluginDump dump) {
            foreach (ModOption option in Options) {
                option.Plugins.Add(dump);
            }
        }
    }
}