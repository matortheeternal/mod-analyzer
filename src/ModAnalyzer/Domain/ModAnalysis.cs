using System.Collections.Generic;
using Newtonsoft.Json;

namespace ModAnalyzer.Domain
{
    public class ModAnalysis
    {
        [JsonProperty(PropertyName = "mod_options")]
        public List<ModOption> ModOptions { get; } = new List<ModOption>();
    }
}
