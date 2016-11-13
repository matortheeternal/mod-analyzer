using System.Collections.Generic;

namespace ModAnalyzer.Domain
{
    public class PluginDump
    {
        public string FileName { get; set; }
        public bool IsESM { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string CrcHash { get; set; }
        public string FileSize { get; set; }
        public string RecordCount { get; set; }
        public string OverrideCount { get; set; }
        public List<string> DummyMasters { get; set; } = new List<string>();
        public List<MasterPlugin> MasterPlugins { get; set; } = new List<MasterPlugin>();
        public List<PluginRecordGroup> PluginRecordGroups { get; set; } = new List<PluginRecordGroup>();
        public List<OverrideRecord> Overrides { get; set; } = new List<OverrideRecord>();
        public List<PluginError> PluginErrors { get; set; } = new List<PluginError>();
    }
}
