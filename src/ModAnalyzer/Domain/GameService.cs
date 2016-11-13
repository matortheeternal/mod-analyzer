using System.Linq;
using IniParser;

namespace ModAnalyzer.Domain
{
    public static class GameService
    {
        private static readonly Game[] Games =
        {
            new Game(GameEnum.FalloutNV, "Fallout New Vegas", "FalloutNV", "FalloutNV.exe", string.Empty, 22380, 2028016), new Game(GameEnum.Fallout3, "Fallout 3", "Fallout3", "Fallout3.exe", string.Empty, 22300, 22370),
            new Game(GameEnum.Oblivion, "Oblivion", "Oblivion", "Oblivion.exe", string.Empty, 22330, 900883), new Game(GameEnum.Skyrim, "Skyrim", "Skyrim", "TESV.exe", string.Empty, 72850),
            new Game(GameEnum.Fallout4, "Fallout 4", "Fallout4", "Fallout4.exe", string.Empty, 377160)
        };

        public static Game GetGame(GameEnum gameMode)
        {
            return Games.FirstOrDefault(game => game.GameMode == gameMode);
        }

        public static string GetGamePath(Game game)
        {
            var fileIniDataParser = new FileIniDataParser();
            var settings = fileIniDataParser.ReadFile("settings.ini");
            var key = game.GameName + "Path";
            key = char.ToLowerInvariant(key[0]) + key.Substring(1);
            return settings["Games"][key];
        }
    }
}
