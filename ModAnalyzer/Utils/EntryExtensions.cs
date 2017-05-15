using System.IO;
using SevenZipExtractor;

namespace ModAnalyzer.Utils {
    public static class EntryExtensions {
        public static string GetPath(this Entry archiveEntry) {
            return archiveEntry.FileName.Replace("/", @"\");
        }

        public static string GetEntryExtension(this Entry archiveEntry) {
            return Path.GetExtension(archiveEntry.FileName).ToUpper();
        }

        public static void ExtractToDirectory(this Entry archiveEntry, string outputDirectory) {
            archiveEntry.Extract(Path.Combine(outputDirectory, Path.GetFileName(archiveEntry.FileName)));
        }
    }
}