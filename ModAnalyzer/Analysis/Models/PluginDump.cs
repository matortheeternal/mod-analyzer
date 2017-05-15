using System;
using System.Collections.Generic;

namespace ModAnalyzer.Analysis.Models {
    public class PluginDump {
        public string filename { get; set; }
        public bool is_esm { get; set; }
        public bool used_dummy_plugins { get; set; }
        public string description { get; set; }
        public string author { get; set; }
        public string crc_hash { get; set; }
        public string file_size { get; set; }
        public string record_count { get; set; }
        public string override_count { get; set; }
        public List<String> dummy_masters { get; set; }
        public List<MasterPlugin> master_plugins { get; set; }
        public List<PluginRecordGroup> plugin_record_groups { get; set; }
        public List<OverrideRecord> overrides { get; set; }
        public List<PluginError> plugin_errors { get; set; }
        public PluginDump() {
            dummy_masters = new List<string>();
            master_plugins = new List<MasterPlugin>();
            plugin_record_groups = new List<PluginRecordGroup>();
            overrides = new List<OverrideRecord>();
            plugin_errors = new List<PluginError>();
        }
    }
}