using Newtonsoft.Json;
using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModAnalyzer.Utils;
using ModAnalyzer.Analysis.Models;
using ModAnalyzer.Domain.Services;
using ModAnalyzer.Analysis.Events;
using System.Windows.Forms;

namespace ModAnalyzer.Analysis.Services {
    public class PluginAnalyzer {
        private readonly BackgroundWorker _backgroundWorker;

        public PluginAnalyzer(BackgroundWorker backgroundWorker) {
            _backgroundWorker = backgroundWorker;
        }

        private void ModDump_MessageReported(object sender, MessageReportedEventArgs e) {
            _backgroundWorker.ReportMessage(e.Message, e.IsStatusMessage);
        }

        public PluginDump GetPluginDump(string pluginPath) {
            DialogResult response;
            try {
                ModDump.MessageReported += ModDump_MessageReported;
                _backgroundWorker.ReportMessage(" ", false);
                _backgroundWorker.ReportMessage("Getting plugin dump for " + pluginPath + "...", true);
                MovePluginToData(pluginPath);
                return AnalyzePlugin(Path.GetFileName(pluginPath));
            }
            catch (Exception x) {
                _backgroundWorker.ReportMessage("Failed to analyze plugin.", false);
                _backgroundWorker.ReportMessage("Exception: " + x.Message, false);
                response = RetryPluginBox(x);
            }
            finally {
                ModDump.MessageReported -= ModDump_MessageReported;
                RevertPlugin(pluginPath);
                _backgroundWorker.ReportMessage(" ", false);
            }

            if (response == DialogResult.Retry) {
                return GetPluginDump(pluginPath);
            } else if (response == DialogResult.Abort) {
                throw new Exception("User aborted analysis.");
            } else {
                return null;
            }
        }

        private DialogResult RetryPluginBox(Exception exception) {
            string title = "Failed to analyze plugin";
            string message = string.Format("{0}: {1}\n\nRetry?", title, exception.Message);
            return MessageBox.Show(message, title, MessageBoxButtons.AbortRetryIgnore);
        }

        public List<string> GetMissingMasterFiles(string pluginPath) {
            string gameDataPath = GameService.DataPath;
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fullPluginPath = Path.Combine(exePath, pluginPath);
            string str = ModDump.DumpMasters(fullPluginPath);
            List<string> masterFileNames = str.ToString().Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();
            return masterFileNames.FindAll(fileName => !File.Exists(Path.Combine(gameDataPath, fileName)));
        }

        private void MovePluginToData(string pluginPath) {
            string dataPath = GameService.DataPath;
            string pluginFileName = Path.GetFileName(pluginPath);
            string dataPluginPath = Path.Combine(dataPath, pluginFileName);
            if (File.Exists(dataPluginPath) && !File.Exists(dataPluginPath + ".bak")) {
                File.Move(dataPluginPath, dataPluginPath + ".bak");
            }

            string fullPluginPath = Path.Combine(PathExtensions.GetProgramPath(), pluginPath);
            File.Move(fullPluginPath, dataPluginPath);
        }

        public PluginDump AnalyzePlugin(string pluginFileName) {
            _backgroundWorker.ReportMessage("Analyzing " + pluginFileName + "...\n", true);
            ModDump.DumpPlugin(pluginFileName);
            ModDump.RaiseLastModDumpError();

            // throw exception if dump json is empty
            string json = ModDump.GetDumpResult();
            if (json.Length < 3) {
                throw new Exception("Failed to analyze plugin " + pluginFileName);
            }

            // deserialize and return plugin dump
            return JsonConvert.DeserializeObject<PluginDump>(json.ToString());
        }

        public void RevertPlugin(string pluginPath) {
            try {
                string dataPath = GameService.DataPath;
                string pluginFileName = Path.GetFileName(pluginPath);
                string pluginDataPath = Path.Combine(dataPath, pluginFileName);
                string oldPluginDataPath = pluginDataPath + ".bak";

                if (File.Exists(pluginDataPath))
                    File.Delete(pluginDataPath);
                if (File.Exists(oldPluginDataPath))
                    File.Move(oldPluginDataPath, pluginDataPath);
            }
            catch (Exception e) {
                _backgroundWorker.ReportMessage("Failed to revert plugin!", false);
                _backgroundWorker.ReportMessage("!!! Please manually revert " + Path.GetFileName(pluginPath), false);
                _backgroundWorker.ReportMessage("Exception:" + e.Message, false);
            }
        }

    }
}