using System.Collections.Generic;
using Newtonsoft.Json;

namespace ModAnalyzer.Domain
{
    public class ModAnalysis
    {
        public ModAnalysis()
        {
            ModOptions = new List<ModOption>();
        }

        [JsonProperty(PropertyName = "mod_options")]
        public List<ModOption> ModOptions { get; set; }
    }
}
