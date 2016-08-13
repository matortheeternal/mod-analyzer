using BA2Lib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using libbsa;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using ModAnalyzer.Utils;
using ModAssetMapper;
using Newtonsoft.Json;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels
{
    // TODO: move a lot of this out of the ViewModel into a service class
    public class AnalysisViewModel : ViewModelBase
    {
        private ModAnalysis _modAnalysis;
        private List<string> _extractedPlugins;
        private List<IArchiveEntry> _plugins;
        private Game _game;
        private BackgroundWorker _assetArchiveAnalyzerBackgroundWorker;

        public ICommand ResetCommand { get; set; }
        public ICommand ViewOutputCommand { get; set; }

        private string _log;

        public string Log
        {
            get { return _log; }
            set { Set(nameof(Log), ref _log, value); }
        }

        private string _progressMessage;

        public string ProgressMessage
        {
            get { return _progressMessage; }
            set { Set(nameof(ProgressMessage), ref _progressMessage, value); }
        }
        
        public AnalysisViewModel()
        {
            _modAnalysis = new ModAnalysis();
            _extractedPlugins = new List<string>();
            _plugins = new List<IArchiveEntry>();

            _assetArchiveAnalyzerBackgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            _assetArchiveAnalyzerBackgroundWorker.ProgressChanged += _assetArchiveAnalyzerBackgroundWorker_ProgressChanged;
            _assetArchiveAnalyzerBackgroundWorker.DoWork += _assetArchiveAnalyzerBackgroundWorker_DoWork;

            Directory.CreateDirectory("output");
            Directory.CreateDirectory(@".\bsas");

            // set game mode to Skyrim
            // TODO: Make this dynamic from GUI
            ModDump.StartModDump();
            _game = GameService.GetGame("Skyrim");
            ModDump.SetGameMode(_game.gameMode);

            ResetCommand = new RelayCommand(() => MessengerInstance.Send(new NavigationMessage(Page.Home)));
            ViewOutputCommand = new RelayCommand(() => Process.Start("output"));

            MessengerInstance.Register<FileSelectedMessage>(this, OnFileSelectedMessage);
        }

        private void _assetArchiveAnalyzerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = e.Argument as string;

            using (IArchive archive = ArchiveFactory.Open(@path))
            {
                (sender as BackgroundWorker).ReportProgress(0, new UIMessage(MessageType.ProgressMessage, "Analyzing archive entries..."));

                foreach (IArchiveEntry modArchiveEntry in archive.Entries)
                {
                    if (modArchiveEntry.IsDirectory)
                        continue;

                    ProcessModArchiveEntry(modArchiveEntry, sender as BackgroundWorker);
                }

                SaveOutputFile(path);
            }
        }

        private void _assetArchiveAnalyzerBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UIMessage uiMessage = e.UserState as UIMessage;

            if (uiMessage.MessageType == MessageType.LogMessage)
                App.Current.Dispatcher.BeginInvoke((Action)(() => AddLogMessage(uiMessage.Message)));
            else
                App.Current.Dispatcher.BeginInvoke((Action)(() => ProgressMessage = uiMessage.Message));
        }

        private void AddLogMessage(string message)
        {
            Log += message + Environment.NewLine;
        }

        private void OnFileSelectedMessage(FileSelectedMessage message)
        {
            Log = string.Empty;

            ProgressMessage = "Loading " + message.FilePath + "...";

            try
            {
                _assetArchiveAnalyzerBackgroundWorker.RunWorkerAsync(message.FilePath);
            }
            catch (Exception e)
            {
                AddLogMessage("Failed to analyze archive.");
                AddLogMessage("Exception:" + e.Message);
            }
        }

        private void SaveOutputFile(string filePath)
        {
            string filename = Path.Combine("output", Path.GetFileNameWithoutExtension(filePath));

            ProgressMessage = "Saving JSON to " + filename + ".json...";
            File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(_modAnalysis));
            ProgressMessage = "All done.  JSON file saved to " + filename + ".json";
        }

        private void ProcessModArchiveEntry(IArchiveEntry modArchiveEntry, BackgroundWorker backgroundWorker)
        {
            string entryPath = modArchiveEntry.GetEntryPath();
            _modAnalysis.assets.Add(entryPath);

            //AddLogMessage(entryPath);
            backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, entryPath));
            
            switch (modArchiveEntry.GetEntryExtesion())
            {
                case ".BA2":
                case ".BSA":
                    //PostProgressMessage("Extracting " + modArchiveEntry.GetEntryExtesion() + " at " + entryPath);
                    backgroundWorker.ReportProgress(0, new UIMessage(MessageType.ProgressMessage, "Extracting " + modArchiveEntry.GetEntryExtesion() + " at " + entryPath));
                    AnalyzeAssetArchive(modArchiveEntry, backgroundWorker);
                    break;
                case ".ESP":
                case ".ESM":
                    // Don't analyze the plugin yet. All plugins will be analyzed after asset archives.
                    _plugins.Add(modArchiveEntry); 
                    break;
            }

            if (_plugins.Any())
                AnalyzePlugins(backgroundWorker);
        }

        public void AnalyzePlugins(BackgroundWorker backgroundWorker)
        {
            //AddLogMessage("Extracting and analyzing plugins...");
            backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, "Extracting and analyzing plugins..."));

            foreach (IArchiveEntry plugin in _plugins)
            {
                try
                {
                    ProcessPlugin(plugin, backgroundWorker);
                }
                catch (Exception e)
                {
                    //AddLogMessage("Failed to analyze plugin.");
                    //AddLogMessage("Exception:" + e.Message);
                    backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, "Failed to analyze plugin."));
                    backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, "Exception: " + e.Message));
                }
            }
        }

        // TODO: BackgroundWorker here
        private void AnalyzeAssetArchive(IArchiveEntry assetArchiveEntry, BackgroundWorker backgroundWorker)
        {
            assetArchiveEntry.WriteToDirectory(@".\bsas", ExtractOptions.Overwrite);

            //PostProgressMessage(assetArchiveEntry.GetEntryExtesion() + " extracted, analyzing entries...");
            backgroundWorker.ReportProgress(0, new UIMessage(MessageType.ProgressMessage, assetArchiveEntry.GetEntryExtesion() + " extracted, analyzing entries..."));

            string assetArchivePath = Path.Combine("bsas", assetArchiveEntry.Key);

            string[] assets;

            if (assetArchiveEntry.GetEntryExtesion().Equals(".bsa", StringComparison.InvariantCultureIgnoreCase))
                assets = GetBSAAssets(assetArchivePath);
            else
                assets = GetBA2Assets(assetArchivePath);

            List<string> assetEntryPaths = assets.Select(asset => Path.Combine(assetArchiveEntry.Key, asset)).ToList();
            _modAnalysis.assets.AddRange(assetEntryPaths);
            //AddLogMessages(assetEntryPaths);
            foreach (string asset in assetEntryPaths)
                backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, asset));
        }

        private string[] GetBSAAssets(string bsaPath)
        {
            string[] entries = null;

            BSANET bsaManager = new BSANET();

            if (bsaManager.bsa_open(bsaPath) == 0)
                entries = bsaManager.bsa_get_assets(".*");

            bsaManager.bsa_close();

            return entries;
        }

        private string[] GetBA2Assets(string ba2Path)
        {
            string[] entries = null;

            using (BA2NET ba2Manager = new BA2NET())
            {
                if (ba2Manager.Open(ba2Path))
                    entries = ba2Manager.GetNameTable();
            }

            return entries;
        }

        private void ProcessPlugin(IArchiveEntry entry, BackgroundWorker backgroundWorker)
        {
            try
            {
                ExtractPlugin(entry, backgroundWorker);
                AnalyzePlugin(entry, backgroundWorker);
                RevertPlugin(entry, backgroundWorker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void RevertPlugin(IArchiveEntry entry, BackgroundWorker backgroundWorker)
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
                backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, "Failed to revert plugin!"));
                backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, "!!! Please manually revert " + Path.GetFileName(entry.Key) + "!!!"));
                backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, "Exception:" + e.Message));
                //AddLogMessage("Failed to revert plugin!");
                //AddLogMessage("!!! Please manually revert " + Path.GetFileName(entry.Key) + "!!!"); // TODO: show dialog message
                //AddLogMessage("Exception:" + e.Message);
            }
        }

        public void ExtractPlugin(IArchiveEntry entry, BackgroundWorker backgroundWorker)
        {
            string gameDataPath = GameService.GetGamePath(_game);
            string pluginFileName = Path.GetFileName(entry.Key);
            string pluginFilePath = gameDataPath + pluginFileName; // TODO: use Path.Combine
            bool alreadyExtracted = _extractedPlugins.Contains(pluginFilePath);
            
            if (!alreadyExtracted)
            {
                backgroundWorker.ReportProgress(0, new UIMessage(MessageType.ProgressMessage, "Extracting " + entry.Key));

                _extractedPlugins.Add(pluginFilePath);

                if (File.Exists(pluginFilePath) && !File.Exists(pluginFilePath + ".bak"))
                    File.Move(pluginFilePath, pluginFilePath + ".bak");
            }
            
            entry.WriteToDirectory(gameDataPath, ExtractOptions.Overwrite);
        }
        
        public void AnalyzePlugin(IArchiveEntry entry, BackgroundWorker backgroundWorker)
        {
            backgroundWorker.ReportProgress(0, new UIMessage(MessageType.ProgressMessage, "Analyzing " + entry.Key));

            // prepare mod dump and message buffer
            StringBuilder message = new StringBuilder(4 * 1024 * 1024);

            // prepare plugin file for dumping
            if (!ModDump.Prepare(Path.GetFileName(entry.Key)))
            {
                ModDump.GetBuffer(message, message.Capacity);
                //AddLogMessage(message.ToString());
                backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, message.ToString()));
                return;
            }

            // dump the plugin file
            StringBuilder json = new StringBuilder(4 * 1024 * 1024); // 4MB maximum dump size
            if (!ModDump.Dump(json, json.Capacity))
            {
                ModDump.GetBuffer(message, message.Capacity);
                //AddLogMessage(message.ToString());
                backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, message.ToString()));
                return;
            }

            PluginDump pluginDump = JsonConvert.DeserializeObject<PluginDump>(json.ToString());
            _modAnalysis.plugins.Add(pluginDump);

            // log the results
            // TODO: This should be handled better.
            ModDump.GetBuffer(message, message.Capacity);
            //AddLogMessage(message.ToString());
            backgroundWorker.ReportProgress(0, new UIMessage(MessageType.LogMessage, message.ToString()));
            ModDump.FlushBuffer();
        }
    }
}
