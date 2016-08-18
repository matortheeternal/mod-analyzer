using System.ComponentModel;

namespace ModAnalyzer.Domain
{
    internal class PluginAnalyzer
    {
        private readonly BackgroundWorker _backgroundWorker;
        private readonly Game _game;

        public PluginAnalyzer(BackgroundWorker backgroundWorker)
        {
            _backgroundWorker = backgroundWorker;

            ModDump.StartModDump();
            _game = GameService.GetGame("Skyrim");
            ModDump.SetGameMode(_game.gameMode);
        }
    }
}