using System;
using System.IO;
using System.Linq;

namespace ModAnalyzer.Analysis.Services {
    public static class ArchiveHelpers {
        public static readonly string[] PluginExtensions = { ".ESP", ".ESM" };
        public static readonly string[] ArchiveExtensions = { ".BA2", ".BSA" };

        public static bool ShouldExtract(string fileName) {
            string ext = Path.GetExtension(fileName);
            return PluginExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase) ||
                    ArchiveExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsPlugin(string fileName) {
            string ext = Path.GetExtension(fileName);
            return PluginExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsArchive(string fileName) {
            string ext = Path.GetExtension(fileName);
            return ArchiveExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }
    }
}
