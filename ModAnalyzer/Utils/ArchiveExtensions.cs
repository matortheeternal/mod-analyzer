using SevenZipExtractor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModAnalyzer.Utils {
    public static class ArchiveExtensions {
        public static List<string> GetArchiveEntryPaths(this ArchiveFile archive) {
            return archive.Entries.Select(x => x.FileName).ToList();
        }

        public static IList<Entry> FileEntries(this ArchiveFile archive) {
            return archive.Entries.Where(e => !e.IsFolder).ToList();
        }

        public static List<string> GetArchiveDirectories(this ArchiveFile archive) {
            return archive.Entries.Where(x => x.IsFolder).Select(x => x.FileName).ToList();
        }

        public static List<string> GetArchiveFiles(this ArchiveFile archive) {
            return archive.Entries.Where(x => !x.IsFolder).Select(x => x.FileName).ToList();
        }

        public static List<string> GetLevelDirectories(this ArchiveFile archive, int level) {
            return GetArchiveDirectories(archive).Where(x => PathExtensions.GetLevel(x) == level).ToList();
        }

        public static List<string> GetLevelFiles(this ArchiveFile archive, int level) {
            return GetArchiveFiles(archive).Where(x => PathExtensions.GetLevel(x) == level).ToList();
        }

        public static Entry FindArchiveEntry(this ArchiveFile archive, string path) {
            foreach (Entry entry in archive.Entries) {
                if (entry.FileName.EndsWith(path, StringComparison.OrdinalIgnoreCase)) {
                    return entry;
                }
            }
            return null;
        }

        public static Entry FindExactArchiveEntry(this ArchiveFile archive, string path) {
            foreach (Entry entry in archive.Entries) {
                if (entry.FileName.Equals(path, StringComparison.OrdinalIgnoreCase)) {
                    return entry;
                }
            }
            return null;
        }

        public static List<string> GetImmediateChildren(this ArchiveFile archive, string path, bool targetDirectories) {
            List<string> children = new List<string>();
            int targetLevel = PathExtensions.GetLevel(path) + 1;
            List<string> searchPaths = targetDirectories ? archive.GetArchiveDirectories() : archive.GetArchiveEntryPaths();
            foreach (string searchPath in searchPaths) {
                if (searchPath.StartsWith(path) && PathExtensions.GetLevel(searchPath) == targetLevel) {
                    children.Add(searchPath);
                }
            }
            return children;
        }

        public static string GetBaseDirectory(this ArchiveFile archive) {
            string baseDirectory = "";
            int level = 1;
            while (archive.GetLevelFiles(level).Count == 0) {
                List<string> topLevelDirectories = archive.GetLevelDirectories(level);
                if (topLevelDirectories.Count == 1) {
                    baseDirectory = topLevelDirectories[0];
                    level++;
                }
                else {
                    break;
                }
            }
            return baseDirectory;
        }

        public static string GetOutputPath(Entry entry, string destination, string baseDir, string outputDir = null) {
            if (!string.IsNullOrEmpty(baseDir)) {
                if (!string.IsNullOrEmpty(outputDir)) {
                    return Path.Combine(destination, entry.FileName.Replace(baseDir, outputDir));
                }
                else {
                    return Path.Combine(destination, entry.FileName.Remove(0, baseDir.Length + 1));
                }
            }
            else {
                return Path.Combine(destination, entry.FileName);
            }
        }

        public static bool SkipEntry(Entry entry, string baseDir) {
            if (string.IsNullOrEmpty(baseDir)) return false;
            return !entry.FileName.StartsWith(baseDir) || entry.FileName.Equals(baseDir) || entry.FileName.StartsWith(Path.Combine(baseDir, "src"));
        }

        public static void ExtractTo(this ArchiveFile archive, string destination) {
            archive.Extract(delegate (Entry entry) {
                if (entry.IsFolder) return null;
                return Path.Combine(destination, entry.FileName);
            });
        }

        public static void ExtractTo(this ArchiveFile archive, string destination, string baseDir, string outputDir = null) {
            archive.Extract(delegate (Entry entry) {
                if (entry.IsFolder || SkipEntry(entry, baseDir)) return null;
                return GetOutputPath(entry, destination, baseDir, outputDir);
            });
        }
    }
}