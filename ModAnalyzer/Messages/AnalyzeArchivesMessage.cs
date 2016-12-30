using ModAnalyzer.Domain;
using System.Collections.Generic;

namespace ModAnalyzer.Messages {
    public class AnalyzeArchivesMessage : NavigationMessage {
        public List<ModOption> ModOptions { get; set; }

        public AnalyzeArchivesMessage(List<ModOption> archiveModOptions) : base(Page.Analysis) {
            ModOptions = archiveModOptions;
        }
    }
}
