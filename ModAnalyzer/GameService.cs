using System;
using System.Collections.Generic;

namespace ModAnalyzer
{
    public static class GameService 
    {
        public static string dataPath { get; set; }
        public static Game game { get; set; }

        private static Game[] gameModes = {
            new Game { longName = "Skyrim", gameName = "Skyrim", gameMode = 4, exeName = "TESV.exe", appIDs = "72850" },
            new Game { longName = "Oblivion", gameName = "Oblivion", gameMode = 3, exeName = "Oblivion.exe", appIDs = "22330,900883" },
            new Game { longName = "Fallout New Vegas", gameName = "FalloutNV", gameMode = 0, exeName = "FalloutNV.exe", appIDs = "22380,2028016" },
            new Game { longName = "Fallout 3", gameName = "Fallout3", gameMode = 1, exeName = "Fallout3.exe", appIDs = "22300,22370" },
            new Game { longName = "Fallout 4", gameName = "Fallout4", gameMode = 5, exeName = "Fallout4.exe", appIDs = "377160" }
        };

        public static Game getGame(String gameName) {
            for (int i = 0; i < gameModes.Length; i++) {
                if (gameModes[i].gameName == gameName) {
                    return gameModes[i];
                }
            }
            return null;
        }
    }
}
