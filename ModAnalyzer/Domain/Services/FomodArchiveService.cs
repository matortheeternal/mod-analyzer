using System;
using ModAnalyzer.Utils;
using SevenZipExtractor;

namespace ModAnalyzer.Domain.Services {
    public static class FomodArchiveService {
        public static Entry GetFomodConfig(ArchiveFile archive) {
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