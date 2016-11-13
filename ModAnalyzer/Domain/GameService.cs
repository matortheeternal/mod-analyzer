using System;
using System.Linq;
using IniParser;

namespace ModAnalyzer.Domain
{
    public static class GameService
    {
        private static readonly Game[] Games =
        {
            new Game
            {
                LongName = "Fallout New Vegas", GameName = "FalloutNV", GameMode = 0, ExeName = "FalloutNV.exe", AppIDs = "22380,2028016"
            },
            new Game
            {
                LongName = "Fallout 3", GameName = "Fallout3", GameMode = 1, ExeName = "Fallout3.exe", AppIDs = "22300,22370"
            },
            new Game
            {
                LongName = "Oblivion", GameName = "Oblivion", GameMode = 2, ExeName = "Oblivion.exe", AppIDs = "22330,900883"
            },
            new Game
            {
                LongName = "Skyrim", GameName = "Skyrim", GameMode = 3, ExeName = "TESV.exe", AppIDs = "72850"
            },
            new Game
            {
                LongName = "Fallout 4", GameName = "Fallout4", GameMode = 4, ExeName = "Fallout4.exe", AppIDs = "377160"
            }
        };

        public static string DataPath { get; set; }

        public static Game GetGame(string gameName)
        {
            return Games.FirstOrDefault(game => game.GameName.Equals(gameName, StringComparison.OrdinalIgnoreCase));
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
