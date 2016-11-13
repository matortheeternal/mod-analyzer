using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using ModAnalyzer.Utils;
using ModAnalyzer.Wrappers;
using Newtonsoft.Json;
using SharpCompress.Archive;

namespace ModAnalyzer.Domain
{
    internal class PluginAnalyzer
    {
        private const int MaxDumpSize = 0x400000; // 4MB maximum dump size
        private readonly BackgroundWorker _backgroundWorker;
        private readonly Game _game;

        public PluginAnalyzer(BackgroundWorker backgroundWorker)
        {
            _backgroundWorker = backgroundWorker;
            if (ModDumpWrapper.Started)
                return;
            ModDumpWrapper.Started = true;
            ModDumpWrapper.StartModDump();
            _game = GameService.GetGame(GameEnum.Fallout4); // TODO: remove hardcoding (requires making a gui component from which the user can choose the game mode)
            ModDumpWrapper.SetGameMode((int) _game.GameMode);
        }

        public PluginDump GetPluginDump(IArchiveEntry entry)
        {
            try
            {
                _backgroundWorker.ReportMessage(Environment.NewLine + "Getting plugin dump for " + entry.Key + "...", true);
                ExtractPlugin(entry);
                return AnalyzePlugin(entry);
            }
            catch (Exception exception)
            {
                _backgroundWorker.ReportMessage("Failed to analyze plugin.");
                _backgroundWorker.ReportMessage("Exception: " + exception.Message);
                return null;
            }
            finally
            {
                _backgroundWorker.ReportMessage(" ");
                RevertPlugin(entry);
            }
        }

        public void ExtractPlugin(IArchiveEntry entry)
        {
            var gameDataPath = GameService.GetGamePath(_game);
            var pluginFileName = Path.GetFileName(entry.Key);
            if (string.IsNullOrEmpty(pluginFileName))
                throw new ArgumentNullException(nameof(pluginFileName));
            var pluginFilePath = Path.Combine(gameDataPath, pluginFileName);
            _backgroundWorker.ReportMessage("Extracting " + entry.Key + "...", true);
            if (File.Exists(pluginFilePath) && !File.Exists(pluginFilePath + ".bak"))
                File.Move(pluginFilePath, pluginFilePath + ".bak");
            entry.WriteToDirectory(gameDataPath);
        }

        private void GetModDumpMessages(StringBuilder message)
        {
            ModDumpWrapper.GetBuffer(message, message.Capacity);
            if (message.Length < 1)
                return;
            var messageString = message.ToString();
            if (messageString.EndsWith("\n") && !messageString.EndsWith(" \n"))
                messageString = messageString.TrimEnd();
            _backgroundWorker.ReportMessage(messageString);
            ModDumpWrapper.FlushBuffer();
        }

        // TODO: refactor
        public PluginDump AnalyzePlugin(IArchiveEntry entry)
        {
            _backgroundWorker.ReportMessage("Analyzing " + entry.Key + "..." + Environment.NewLine, true);
            var message = new StringBuilder(MaxDumpSize);

            // prepare plugin file for dumping
            var dataPath = GameService.GetGamePath(_game);
            var fileName = Path.GetFileName(entry.Key);
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            var filePath = Path.Combine(dataPath, fileName);
            if (!ModDumpWrapper.Prepare(filePath))
            {
                GetModDumpMessages(message);
                return null;
            }

            // dump the plugin file
            var json = new StringBuilder(MaxDumpSize);
            if (!ModDumpWrapper.Dump())
            {
                GetModDumpMessages(message);
                return null;
            }

            while (!ModDumpWrapper.GetDumpResult(json, json.Capacity)) // use a loop to poll for messages until the dump is ready
            {
                GetModDumpMessages(message);
                // wait 100ms between each polling operation so we don't bring things to a standstill with this while loop
                // we can do this without locking up the UI because this is happening in a background worker
                //Thread.Sleep(100);
            }
            GetModDumpMessages(message); // get any remaining messages

            // TODO: check if it's need - throw exception if dump json is empty
            //if (json.Length < 3)
            //   throw new Exception("Failed to analyze plugin " + fileName);

            return JsonConvert.DeserializeObject<PluginDump>(json.ToString()); // deserialize and return plugin dump
        }

        public void RevertPlugin(IArchiveEntry entry)
        {
            try
            {
                var dataPath = GameService.GetGamePath(_game);
                var fileName = Path.GetFileName(entry.Key);
                if (string.IsNullOrEmpty(fileName))
                    throw new ArgumentNullException(nameof(fileName));
                var filePath = Path.Combine(dataPath, fileName);
                File.Delete(filePath);

                if (File.Exists(filePath + ".bak"))
                    File.Move(filePath + ".bak", filePath);
            }
            catch (Exception e)
            {
                _backgroundWorker.ReportMessage("Failed to revert plugin!");
                _backgroundWorker.ReportMessage("!!! Please manually revert " + Path.GetFileName(entry.Key) + "!!!");
                _backgroundWorker.ReportMessage("Exception:" + e.Message);
            }
        }
    }
}
