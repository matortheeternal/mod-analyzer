using System.Collections.Generic;
using ModAnalyzer.Domain;

namespace ModAnalyzer.Messages
{
    public class ArchiveModOptionsSelectedMessage : NavigationMessage
    {
        public ArchiveModOptionsSelectedMessage(List<ModOption> archiveModOptions) : base(Page.Analysis)
        {
            ModOptions = archiveModOptions;
        }

        public List<ModOption> ModOptions { get; }
    }
}
