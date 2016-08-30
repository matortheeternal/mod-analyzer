using Newtonsoft.Json;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace ModAnalyzer.Domain
{
    internal class PluginAnalyzer
    {
        private readonly BackgroundWorker _backgroundWorker;
        private readonly Game _game;
        private readonly List<string> _extractedPlugins;

        public PluginAnalyzer(BackgroundWorker backgroundWorker)
        {
            _backgroundWorker = backgroundWorker;
            _extractedPlugins = new List<string>();

            ModDump.StartModDump();
            _game = GameService.GetGame("Skyrim"); // TODO: remove hardcoding
            ModDump.SetGameMode(_game.gameMode);
        }

        public PluginDump GetPluginDump(IArchiveEntry entry)
        {
            try
            {
                _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs("Getting plugin dump for " + entry.Key + "..."));
                ExtractPlugin(entry);
                return AnalyzePlugin(entry);
            }
            catch (Exception exception)
            {
                _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs("Failed to analyze plugin."));
                _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs("Exception: " + exception.Message));

                return null;
            }
            finally
            {
                RevertPlugin(entry);
            }
        }

        public void ExtractPlugin(IArchiveEntry entry)
        {
            string gameDataPath = GameService.GetGamePath(_game);
            string pluginFileName = Path.GetFileName(entry.Key);
            string pluginFilePath = gameDataPath + pluginFileName; // TODO: use Path.Combine
            bool alreadyExtracted = _extractedPlugins.Contains(pluginFilePath);

            if (alreadyExtracted)
                return;

            _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateProgressMessageEventArgs("Extracting " + entry.Key));

            _extractedPlugins.Add(pluginFilePath);

            if (File.Exists(pluginFilePath) && !File.Exists(pluginFilePath + ".bak"))
                File.Move(pluginFilePath, pluginFilePath + ".bak");

            entry.WriteToDirectory(gameDataPath, ExtractOptions.Overwrite);
        }

        // TODO: refactor
        public PluginDump AnalyzePlugin(IArchiveEntry entry)
        {
            _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateProgressMessageEventArgs("Analyzing " + entry.Key));

            // prepare mod dump and message buffer
            StringBuilder message = new StringBuilder(4 * 1024 * 1024);

            // prepare plugin file for dumping
            if (!ModDump.Prepare(Path.GetFileName(entry.Key)))
            {
                ModDump.GetBuffer(message, message.Capacity);
                _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs(Environment.NewLine + message.ToString()));
                return null;
            }

            // dump the plugin file
            StringBuilder json = new StringBuilder(4 * 1024 * 1024); // 4MB maximum dump size
            if (!ModDump.Dump())
            {
                ModDump.GetBuffer(message, message.Capacity);
                _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs(Environment.NewLine + message.ToString()));
                return null;
            }

            // use a loop to poll for messages until the dump is ready
            while (!ModDump.GetDumpResult(json, json.Capacity)) {
                ModDump.GetBuffer(message, message.Capacity);
                if (message.Length > 1) {
                    _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs(message.ToString()));
                    ModDump.FlushBuffer();
                }
                // wait 100ms between each polling operation so we don't bring things to a standstill with this while loop
                // we can do this without locking up the UI because this is happening in a background worker
                System.Threading.Thread.Sleep(100);
            }

            return JsonConvert.DeserializeObject<PluginDump>(json.ToString());
        }

        public void RevertPlugin(IArchiveEntry entry)
        {
            try
            {
                string dataPath = GameService.GetGamePath(_game);
                string fileName = Path.GetFileName(entry.Key);
                string filePath = dataPath + fileName;

                File.Delete(filePath);

                if (File.Exists(filePath + ".bak"))
                    File.Move(filePath + ".bak", filePath);
            }
            catch (Exception e)
            {
                _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs("Failed to revert plugin!"));
                _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs("!!! Please manually revert " + Path.GetFileName(entry.Key) + "!!!"));
                _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateLogMessageEventArgs("Exception:" + e.Message));
            }
        }
    }
}