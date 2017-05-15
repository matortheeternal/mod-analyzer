using ModAnalyzer.Domain.Services;
using Newtonsoft.Json;

namespace ModAnalyzer.Domain.Models {
    public class ProgramSetting {
        [JsonProperty(PropertyName = "skyrim_path")]
        public string SkyrimPath { get; set; }
        [JsonProperty(PropertyName = "skyrimse_path")]
        public string SkyrimSEPath { get; set; }

        public ProgramSetting() {
            SkyrimPath = GameService.GetGameDataPath("Skyrim");
            SkyrimSEPath = GameService.GetGameDataPath("SkyrimSE");
        }
    }
}
