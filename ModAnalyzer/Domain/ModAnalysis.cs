using System.Collections.Generic;

namespace ModAnalyzer.Domain
{
    public class ModAnalysis
    {
        public List<ModOption> ModOptions { get; set; }

        public ModAnalysis()
        {
            ModOptions = new List<ModOption>();
        }
    }
}