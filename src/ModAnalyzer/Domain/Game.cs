namespace ModAnalyzer.Domain
{
    public class Game
    {
        public Game(GameEnum gameMode, string longName, string gameName, string exeName, string appName, params int[] appIDs)
        {
            GameMode = gameMode;
            LongName = longName;
            GameName = gameName;
            AppName = appName;
            ExeName = exeName;
            AppIDs = appIDs;
        }

        public string LongName { get; }
        public string GameName { get; }
        public GameEnum GameMode { get; }
        public string AppName { get; }
        public string ExeName { get; }
        public int[] AppIDs { get; }
    }
}
