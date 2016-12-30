using ModAnalyzer.Domain;
using System.Collections.Generic;

namespace ModAnalyzer.Messages {
    public class MissingMastersMessage : NavigationMessage {
        public List<MissingMaster> MissingMasters { get; set; }
        public List<ModOption> ModOptions { get; set; }

        public MissingMastersMessage(List<MissingMaster> missingMasters, List<ModOption> modOptions) : base(Page.PluginMasters) {
            MissingMasters = missingMasters;
            ModOptions = modOptions;
        }
    }
}