using ModAnalyzer.Analysis.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ModAnalyzer.Analysis.Models {
    public class ModAnalysis {
        [JsonProperty(PropertyName = "mod_options")]
        public List<ModOption> ModOptions { get; set; }

        public ModAnalysis() {
            ModOptions = new List<ModOption>();
        }
    }
}