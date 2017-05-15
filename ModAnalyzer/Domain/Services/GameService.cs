using System;
using System.Linq;
using ModAnalyzer.Domain.Models;
using System.IO;

namespace ModAnalyzer.Domain.Services {
    public static class GameService {
        public static Game CurrentGame { get; set; }
        public static string DataPath {
            get {
                return SettingsService.GamePath(CurrentGame.gameName);
            }
        }

        // TODO: Move this to a JSON file that is loaded at runtime
        // (even better if it can be packaged with the executable as a resource)
        private static Game[] _games =
            {
                new Game { longName = "Fallout New Vegas", gameName = "FalloutNV", regName = "FalloutNV", abbrName = "fnv", gameMode = 0, exeName = "FalloutNV.exe", appIDs = "22380,2028016" },
                new Game { longName = "Fallout 3", gameName = "Fallout3", regName = "Fallout3", abbrName = "fo3", gameMode = 1, exeName = "Fallout3.exe", appIDs = "22300,22370" },
                new Game { longName = "Oblivion", gameName = "Oblivion", regName = "Oblivion", abbrName = "ob", gameMode = 2, exeName = "Oblivion.exe", appIDs = "22330,900883" },
                new Game { longName = "Skyrim", gameName = "Skyrim", regName = "Skyrim", abbrName = "sk", gameMode = 3, gameId = 2, exeName = "TESV.exe", appIDs = "72850" },
                new Game { longName = "Fallout 4", gameName = "Fallout4", abbrName = "fo4", gameMode = 4, exeName = "Fallout4.exe", appIDs = "377160" },
                new Game { longName = "Skyrim Special Edition", gameName = "SkyrimSE", abbrName = "sse", regName = "Skyrim Special Edition", gameMode = 5, gameId = 7, exeName = "SkyrimSE.exe", appIDs = "489830" }
            };

        public static Game GetGame(string gameName) {
            return _games.FirstOrDefault(game => game.gameName.Equals(gameName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string GetGameDataPath(string gameName) {
            Game game = GetGame(gameName);
            string gamePath = game.GetGamePath();
            return string.IsNullOrEmpty(gamePath) ? string.Empty : Path.Combine(gamePath, "Data");
        }

        public static Game GetGameById(int game_id) {
            return _games.FirstOrDefault(game => game.gameId == game_id);
        }

        public static void SetGameById(int game_id) {
            CurrentGame = GetGameById(game_id);
        }

        public static void SetGame(string gameName) {
            CurrentGame = GetGame(gameName);
        }

        public static string GetAbbrName(int gameMode) {
            return _games[gameMode].abbrName;
        }

        public static Game GetGameByLongName(string longName) {
            return _games.FirstOrDefault(game => game.longName.Equals(longName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsValidGamePath(Game targetGame, string gamePath) {
            if (targetGame == null) return false;
            try {
                if (!Path.IsPathRooted(gamePath)) return false;
                return File.Exists(Path.Combine(gamePath, targetGame.exeName));
            }
            catch (Exception) {
                return false;
            }
        }

        public static string[] GetOfficialExts() {
            return new string[] { ".BSA", ".EXE", ".ESM" };
        }
    }
}
