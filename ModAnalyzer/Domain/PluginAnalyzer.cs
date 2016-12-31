using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModAnalyzer.Utils;

namespace ModAnalyzer.Domain {
    internal class PluginAnalyzer {
        private readonly BackgroundWorker _backgroundWorker;

        public PluginAnalyzer(BackgroundWorker backgroundWorker) {
            _backgroundWorker = backgroundWorker;

            if (!ModDump.started) {
                ModDump.started = true;
                ModDump.StartModDump();
                ModDump.SetGameMode(GameService.currentGame.gameMode);
            }
        }

        public PluginDump GetPluginDump(string pluginPath) {
            try {
                _backgroundWorker.ReportMessage(" ", false);
                _backgroundWorker.ReportMessage("Getting plugin dump for " + pluginPath + "...", true);
                MovePluginToData(pluginPath);
                return AnalyzePlugin(Path.GetFileName(pluginPath));
            }
            catch (Exception exception) {
                _backgroundWorker.ReportMessage("Failed to analyze plugin.", false);
                _backgroundWorker.ReportMessage("Exception: " + exception.Message, false);
                return null;
            }
            finally {
                RevertPlugin(pluginPath);
                _backgroundWorker.ReportMessage(" ", false);
            }
        }

        public List<string> GetMissingMasterFiles(string pluginPath) {
            string gameDataPath = GameService.GetCurrentGamePath();
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fullPluginPath = Path.Combine(exePath, pluginPath);
            StringBuilder str = new StringBuilder(16384);
            ModDump.DumpMasters(fullPluginPath, str, 16384);
            List<string> masterFileNames = str.ToString().Split(';').ToList();
            return masterFileNames.FindAll(fileName => !File.Exists(Path.Combine(gameDataPath, fileName)));
        }

        private void GetModDumpMessages(StringBuilder message) {
            ModDump.GetBuffer(message, message.Capacity);
            if (message.Length > 0) {
                string messageString = message.ToString();
                if (messageString.EndsWith("\n") && !messageString.EndsWith(" \n")) {
                    messageString = messageString.TrimEnd();
                }
                _backgroundWorker.ReportMessage(messageString, false);
                ModDump.FlushBuffer();
            }
        }

        private void MovePluginToData(string pluginPath) {
            string dataPath = GameService.GetCurrentGamePath();
            string pluginFileName = Path.GetFileName(pluginPath);
            string dataPluginPath = Path.Combine(dataPath, pluginFileName);
            if (File.Exists(dataPluginPath) && !File.Exists(dataPluginPath + ".bak")) {
                File.Move(dataPluginPath, dataPluginPath + ".bak");
            }
            
            string fullPluginPath = Path.Combine(PathExtensions.GetProgramPath(), pluginPath);
            File.Move(fullPluginPath, dataPluginPath);
        }

        // TODO: refactor
        public PluginDump AnalyzePlugin(string pluginFileName) {
            _backgroundWorker.ReportMessage("Analyzing " + pluginFileName + "...\n", true);
            StringBuilder message = new StringBuilder(4 * 1024 * 1024);

            // prepare plugin file for dumping
            if (!ModDump.Prepare(pluginFileName)) {
                GetModDumpMessages(message);
                return null;
            }

            // dump the plugin file
            StringBuilder json = new StringBuilder(4 * 1024 * 1024); // 4MB maximum dump size
            if (!ModDump.Dump()) {
                GetModDumpMessages(message);
                return null;
            }

            // use a loop to poll for messages until the dump is ready
            while (!ModDump.GetDumpResult(json, json.Capacity)) {
                GetModDumpMessages(message);
                // wait 100ms between each polling operation so we don't bring things to a standstill with this while loop
                // we can do this without locking up the UI because this is happening in a background worker
                System.Threading.Thread.Sleep(100);
            }
            
            // get any remaining messages
            GetModDumpMessages(message);

            // throw exception if dump json is empty
            if (json.Length < 3) {
                throw new Exception("Failed to analyze plugin " + pluginFileName);
            }

            // deserialize and return plugin dump
            return JsonConvert.DeserializeObject<PluginDump>(json.ToString());
        }

        public void RevertPlugin(string pluginPath) {
            try {
                string dataPath = GameService.GetCurrentGamePath();
                string pluginFileName = Path.GetFileName(pluginPath);
                string pluginDataPath = Path.Combine(dataPath, pluginFileName);
                string oldPluginDataPath = pluginPath + ".bak";

                if (File.Exists(pluginDataPath)) {
                    File.Delete(pluginDataPath);
                }

                if (File.Exists(oldPluginDataPath)) {
                    File.Move(oldPluginDataPath, pluginDataPath);
                }
            }
            catch (Exception e) {
                _backgroundWorker.ReportMessage("Failed to revert plugin!", false);
                _backgroundWorker.ReportMessage("!!! Please manually revert " + Path.GetFileName(pluginPath), false);
                _backgroundWorker.ReportMessage("Exception:" + e.Message, false);
            }
        }

    }
}