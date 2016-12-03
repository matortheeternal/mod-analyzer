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
    }
}
