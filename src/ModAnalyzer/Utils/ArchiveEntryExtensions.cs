using System.IO;
using SharpCompress.Archive;

namespace ModAnalyzer.Utils
{
    public static class ArchiveEntryExtensions
    {
        public static string GetEntryPath(this IArchiveEntry archiveEntry)
        {
            return archiveEntry.Key.Replace('/', '\\');
        }

        public static string GetEntryExtension(this IArchiveEntry archiveEntry)
        {
            var extension = Path.GetExtension(GetEntryPath(archiveEntry));
            if (!string.IsNullOrEmpty(extension))
                return extension.ToUpper();
            return string.Empty;
        }
    }
}
