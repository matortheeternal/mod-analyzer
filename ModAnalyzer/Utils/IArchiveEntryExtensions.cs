using SharpCompress.Archive;
using System.IO;

namespace ModAnalyzer.Utils
{
    public static class IArchiveEntryExtensions
    {
        public static string GetEntryPath(this IArchiveEntry archiveEntry)
        {
            return archiveEntry.Key.Replace('/', '\\');
        }

        public static string GetEntryExtension(this IArchiveEntry archiveEntry)
        {
            return Path.GetExtension(GetEntryPath(archiveEntry)).ToUpper();
        }
    }
}
