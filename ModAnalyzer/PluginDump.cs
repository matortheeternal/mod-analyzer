using System;
using System.Collections.Generic;

namespace ModAnalyzer
{
    public class PluginDump
    {
        public string FileName { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string CRC_Hash { get; set; }
        public string File_Size { get; set; }
        public string Record_Count { get; set; }
        public string Override_Count { get; set; }
        public List<String> Master_FileName { get; set; }
        public List<String> Dummy_Masters { get; set; }
        public List<PluginRecordGroup> Plugin_Record_Groups { get; set; }
        public List<OverrideRecord> Overrides { get; set; }
        public List<PluginError> Plugin_Errors { get; set; }

        public PluginDump()
        {
            Master_FileName = new List<string>();
            Dummy_Masters = new List<string>();
            Plugin_Record_Groups = new List<PluginRecordGroup>();
            Overrides = new List<OverrideRecord>();
            Plugin_Errors = new List<PluginError>();
        }
    }
}
