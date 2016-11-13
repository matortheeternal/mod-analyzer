using System.Collections.Generic;
using Newtonsoft.Json;

namespace ModAnalyzer.Domain
{
    /// <summary>
    ///     Represents a logical component of a mod, e.g. the base mod, a patch, or a fomod option.
    /// </summary>
    public class ModOption
    {
        public ModOption()
        {
            Assets = new List<string>();
            Plugins = new List<PluginDump>();
            Size = 0;
        }

        public ModOption(string name, bool Default, bool isFomodOption)
        {
            Name = name;
            this.Default = Default;
            IsFomodOption = isFomodOption;
            Assets = new List<string>();
            Plugins = new List<PluginDump>();
            Size = 0;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }

        [JsonProperty(PropertyName = "default")]
        public bool Default { get; set; }

        [JsonProperty(PropertyName = "is_fomod_option")]
        public bool IsFomodOption { get; set; }

        [JsonProperty(PropertyName = "assets")]
        public List<string> Assets { get; set; }

        [JsonProperty(PropertyName = "plugins")]
        public List<PluginDump> Plugins { get; set; }

        [JsonIgnore]
        public string SourceFilePath { get; set; }

        public static bool IsEmpty(ModOption option)
        {
            return (option.Assets.Count == 0) && (option.Plugins.Count == 0);
        }
    }
}
