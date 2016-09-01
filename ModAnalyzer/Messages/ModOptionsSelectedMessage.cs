using ModAnalyzer.Domain;
using System.Collections.Generic;

namespace ModAnalyzer.Messages {
    public class ModOptionsSelectedMessage : NavigationMessage {
        public ICollection<ModOption> ModOptions { get; set; }

        public ModOptionsSelectedMessage(ICollection<ModOption> modOptions) : base(Page.Analysis) {
            ModOptions = modOptions;
        }
    }
}
