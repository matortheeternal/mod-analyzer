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
        }

        public ModOption(string name, bool @default, bool isFomodOption) : this()
        {
            Name = name;
            Default = @default;
            IsFomodOption = isFomodOption;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("default")]
        public bool Default { get; set; }

        [JsonProperty("is_fomod_option")]
        public bool IsFomodOption { get; set; }

        [JsonProperty("assets")]
        public List<string> Assets { get; set; } = new List<string>();

        [JsonProperty("plugins")]
        public List<PluginDump> Plugins { get; set; } = new List<PluginDump>();

        [JsonIgnore]
        public string SourceFilePath { get; set; }

        public static bool IsEmpty(ModOption option)
        {
            return (option.Assets.Count == 0) && (option.Plugins.Count == 0);
        }
    }
}
