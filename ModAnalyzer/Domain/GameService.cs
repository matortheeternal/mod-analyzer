using System;
using IniParser;
using IniParser.Model;
using System.Linq;

namespace ModAnalyzer {
    public static class GameService {
        public static string dataPath { get; set; }
        private static IniData settings;

        private static Game[] _games =
            {
                new Game { longName = "Fallout New Vegas", gameName = "FalloutNV", gameMode = 0, exeName = "FalloutNV.exe", appIDs = "22380,2028016" },
                new Game { longName = "Fallout 3", gameName = "Fallout3", gameMode = 1, exeName = "Fallout3.exe", appIDs = "22300,22370" },
                new Game { longName = "Oblivion", gameName = "Oblivion", gameMode = 2, exeName = "Oblivion.exe", appIDs = "22330,900883" },
                new Game { longName = "Skyrim", gameName = "Skyrim", gameMode = 3, exeName = "TESV.exe", appIDs = "72850" },
                new Game { longName = "Fallout 4", gameName = "Fallout4", gameMode = 4, exeName = "Fallout4.exe", appIDs = "377160" }
            };

        public static Game GetGame(string gameName) {
            return _games.FirstOrDefault(game => game.gameName.Equals(gameName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static void loadIni() {
            var parser = new FileIniDataParser();
            settings = parser.ReadFile("settings.ini");
        }

        public static string GetGamePath(Game game) {
            FileIniDataParser fileIniDataParser = new FileIniDataParser();
            IniData settings = fileIniDataParser.ReadFile("settings.ini");

            string key = game.gameName + "Path";
            key = Char.ToLowerInvariant(key[0]) + key.Substring(1);
            return settings["Games"][key];
        }
    }
}
