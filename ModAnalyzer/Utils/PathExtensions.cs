using System;
using System.IO;
using System.Reflection;

namespace ModAnalyzer.Utils {
    
    public static class PathExtensions {
        public static int GetLevel(string path) {
            return path.Split('\\').Length;
        }

        public static string AppendDelimiter(string path) {
            if (path.EndsWith(@"\")) {
                return path;
            }
            else {
                return path + @"\";
            }
        }

        public static string FixDelimiters(string path) {
            return path.Replace("/", @"\");
        }

        public static string GetProgramPath() {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static bool InvalidPluginPath(string pluginPath) {
            bool IsMacOSX = pluginPath.IndexOf("__MACOSX", System.StringComparison.OrdinalIgnoreCase) > -1;
            bool IsLODGen = pluginPath.IndexOf(@"textures\terrain\LODGen", StringComparison.OrdinalIgnoreCase) > -1;
            bool IsFaceGen = pluginPath.IndexOf(@"actors\character\FaceGenData\", StringComparison.OrdinalIgnoreCase) > -1;
            return IsLODGen || IsMacOSX || IsFaceGen;
        }
    }
}
