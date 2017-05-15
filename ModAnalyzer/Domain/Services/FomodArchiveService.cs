using System;
using SharpCompress.Archive;
using ModAnalyzer.Utils;

namespace ModAnalyzer.Domain.Services {
    public static class FomodArchiveService {
        public static IArchiveEntry GetFomodConfig(IArchive archive) {
            return archive.FindArchiveEntry(@"fomod\ModuleConfig.xml");
        }

        public static string GetBasePath(string configEntryPath) {
            string configFomodPath = @"fomod\ModuleConfig.xml";
            int index = configEntryPath.IndexOf(configFomodPath, StringComparison.OrdinalIgnoreCase);
            if (index >= 0) {
                return configEntryPath.Remove(index, configFomodPath.Length);
            }
            else {
                return "";
            }
        }
    }
}