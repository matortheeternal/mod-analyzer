using System.Collections.Generic;

namespace ModAnalyzer.Domain.Fomod {
    public class FomodFileEqualityComparer : IEqualityComparer<FomodFile> {
        public bool Equals(FomodFile f1, FomodFile f2) {
            return (f1 == null && f2 == null) ||
                (f1 != null && f2 != null && 
                f1.Source == f2.Source && 
                f1.Destination == f2.Destination && 
                f1.IsFolder == f2.IsFolder);
        }

        public int GetHashCode(FomodFile f) {
            unchecked {
                return 23 * f.Source.GetHashCode() + f.Destination.GetHashCode();
            }
        }
    }
}
