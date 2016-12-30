using ModAnalyzer.Domain;
using System.Collections.Generic;

namespace ModAnalyzer.Messages {
    public class ArchivesClassifiedMessage : NavigationMessage {
        public List<ModOption> ModOptions { get; set; }

        public ArchivesClassifiedMessage(List<ModOption> archiveModOptions) : base(Page.ExtractArchives) {
            ModOptions = archiveModOptions;
        }
    }
}
