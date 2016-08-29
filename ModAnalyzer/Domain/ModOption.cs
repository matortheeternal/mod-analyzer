using System.Collections.Generic;

namespace ModAnalyzer.Domain
{
    /// <summary>
    /// Represents a logical component of a mod, e.g. the base mod, a patch, or a fomod option.
    /// </summary>
    public class ModOption
    {
        public string Name { get; set; }
        public bool IsDefaultOption { get; set; }
        public bool IsFomodOption { get; set; }
        public List<string> Assets { get; set; }
        public List<PluginDump> Plugins { get; set; }

        public ModOption()
        {
            Assets = new List<string>();
            Plugins = new List<PluginDump>();
        }
    }
}