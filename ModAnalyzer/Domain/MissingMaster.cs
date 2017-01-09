namespace ModAnalyzer {
    public class MissingMaster {
        public string FileName { get; set; }
        public string RequiredBy { get; set; }

        public MissingMaster(string filename, string requiredBy) {
            FileName = filename;
            RequiredBy = requiredBy;
        }

        public void AddRequiredBy(string pluginPath) {
            RequiredBy += ", " + pluginPath;
        }
    }
}