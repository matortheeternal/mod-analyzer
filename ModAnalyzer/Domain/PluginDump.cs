using System.Collections.Generic;

namespace ModAnalyzer.Domain
{
    public class PluginDump
    {
        public PluginDump()
        {
            DummyMasters = new List<string>();
            MasterPlugins = new List<MasterPlugin>();
            PluginRecordGroups = new List<PluginRecordGroup>();
            Overrides = new List<OverrideRecord>();
            PluginErrors = new List<PluginError>();
        }

        public string Filename { get; set; }
        public bool IsEsm { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string CrcHash { get; set; }
        public string FileSize { get; set; }
        public string RecordCount { get; set; }
        public string OverrideCount { get; set; }
        public List<string> DummyMasters { get; set; }
        public List<MasterPlugin> MasterPlugins { get; set; }
        public List<PluginRecordGroup> PluginRecordGroups { get; set; }
        public List<OverrideRecord> Overrides { get; set; }
        public List<PluginError> PluginErrors { get; set; }
    }
}
