using System.Collections.Generic;

namespace ModAnalyzer.Domain {
    public class ArchivesExtractedEventArgs {
        public List<MissingMaster> MissingMasters { get; set; }
        public bool HasMissingMasters { get; set; }

        public ArchivesExtractedEventArgs(List<MissingMaster> missingMasters) {
            MissingMasters = missingMasters;
            HasMissingMasters = missingMasters.Count > 0;
        }
    }
}