namespace ModAnalyzer.Analysis.Models {
    public class PluginError {
        public int group { get; set; }
        public string signature { get; set; }
        public int form_id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string data { get; set; }
    }
}
