using System.Collections.Generic;

namespace ModAnalyzer.Domain
{
    public class ModAnalysis
    {
        public List<string> Assets { get; set; }
        public List<PluginDump> Plugins { get; set; }

        public ModAnalysis()
        {
            Assets = new List<string>();
            Plugins = new List<PluginDump>();
        }
    }
}