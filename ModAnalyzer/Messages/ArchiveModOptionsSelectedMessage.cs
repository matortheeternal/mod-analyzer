using ModAnalyzer.Domain;
using System.Collections.Generic;

namespace ModAnalyzer.Messages {
    public class ArchiveModOptionsSelectedMessage : NavigationMessage {
        public List<ModOption> ModOptions { get; set; }

        public ArchiveModOptionsSelectedMessage(List<ModOption> archiveModOptions) : base(Page.Analysis) {
            ModOptions = archiveModOptions;
        }
    }
}
