using System.Collections.Generic;

namespace ModAnalyzer
{
    public class ModAnalysis
    {
        public List<string> assets { get; set; }
        public List<PluginDump> plugins { get; set; }

        public ModAnalysis()
        {
            assets = new List<string>();
            plugins = new List<PluginDump>();
        }
    }
}