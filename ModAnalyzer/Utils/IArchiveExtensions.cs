using SharpCompress.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModAnalyzer.Utils {

    public static class IArchiveExtensions {
        public static List<string> GetArchiveEntryPaths(this IArchive archive) {
            return archive.Entries.Select(x => x.GetPath()).ToList();
        }

        public static List<string> GetArchiveDirectories(this IArchive archive) {
            HashSet<string> directories = new HashSet<string>();
            List<string> entryPaths = archive.GetArchiveEntryPaths();
            entryPaths.ForEach(x => {
                string path = Path.GetDirectoryName(x);
                while (path != "") {
                    directories.Add(path);
                    path = Path.GetDirectoryName(path);
                }
            });
            return directories.ToList();
        }

        public static List<string> GetLevelDirectories(this IArchive archive, int level) {
            List<string> levelDirectories = new List<string>();
            foreach (string dirPath in GetArchiveDirectories(archive)) {
                if (PathExtensions.GetLevel(dirPath) == level) {
                    levelDirectories.Add(dirPath);
                }
            }
            return levelDirectories;
        }

        public static IArchiveEntry FindArchiveEntry(this IArchive archive, string path) {
            foreach (IArchiveEntry entry in archive.Entries) {
                string entryPath = entry.GetPath();
                if (entryPath.EndsWith(path, StringComparison.OrdinalIgnoreCase)) {
                    return entry;
                }
            }
            return null;
        }

        public static List<string> GetImmediateChildren(this IArchive archive, string path, bool targetDirectories) {
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
    }
}