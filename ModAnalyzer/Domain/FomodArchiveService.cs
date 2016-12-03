using System;
using System.IO;
using System.Linq;
using SharpCompress.Archive;
using System.Collections.Generic;
using ModAnalyzer.Utils;

namespace ModAnalyzer.Domain {
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