using BA2Lib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using libbsa;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using ModAnalyzer.Utils;
using Newtonsoft.Json;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
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

        public ICommand ResetCommand { get; set; }
        public ICommand ViewOutputCommand { get; set; }

        private string _log;

        public string Log
        {
            get { return _log; }
            set { Set(nameof(Log), ref _log, value); }
        }

        public AnalysisViewModel()
        {
            _modAnalysis = new ModAnalysis();
            _extractedPlugins = new List<string>();
            _plugins = new List<IArchiveEntry>();

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

        private void PostProgressMessage(string message)
        {
            MessengerInstance.Send(new ProgressMessage(message));
        }

        private void AddLogMessage(string message)
        {
            Log += message + Environment.NewLine;
        }

        private void AddLogMessages(List<string> messages)
        {
            foreach (string message in messages)
                Log += message + Environment.NewLine;
        }

        private void OnFileSelectedMessage(FileSelectedMessage message)
        {
            Log = string.Empty;

            PostProgressMessage("Loading " + message.FilePath + "...");

            try
            {
                GetModArchiveEntryMap(message.FilePath);
            }
            catch (Exception e)
            {
                AddLogMessage("Failed to analyze archive.");
                AddLogMessage("Exception:" + e.Message);
            }

            SaveOutputFile(message.FilePath);
        }

        private void SaveOutputFile(string filePath)
        {
            string filename = Path.Combine("output", Path.GetFileNameWithoutExtension(filePath));

            PostProgressMessage("Saving JSON to " + filename + ".json...");
            File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(_modAnalysis));
            PostProgressMessage("All done.  JSON file saved to " + filename + ".json");
        }

        private void GetModArchiveEntryMap(string path)
        {
            using (IArchive archive = ArchiveFactory.Open(@path))
            {
                PostProgressMessage("Analyzing archive entries...");
                
                foreach (IArchiveEntry modArchiveEntry in archive.Entries)
                {
                    if (modArchiveEntry.IsDirectory)
                        continue;

                    ProcessModArchiveEntry(modArchiveEntry);
                }
            }  
        }

        private void ProcessModArchiveEntry(IArchiveEntry modArchiveEntry)
        {
            string entryPath = modArchiveEntry.GetEntryPath();
            _modAnalysis.assets.Add(entryPath);

            AddLogMessage(entryPath);

            switch (modArchiveEntry.GetEntryExtesion())
            {
                case ".BA2":
                case ".BSA":
                    PostProgressMessage("Extracting " + modArchiveEntry.GetEntryExtesion() + " at " + entryPath);
                    AnalyzeAssetArchive(modArchiveEntry);
                    break;
                case ".ESP":
                case ".ESM":
                    // Don't analyze the plugin yet. All plugins will be analyzed after asset archives.
                    _plugins.Add(modArchiveEntry); 
                    break;
            }

            if (_plugins.Any())
                AnalyzePlugins();
        }

        public void AnalyzePlugins()
        {
            AddLogMessage("Extracting and analyzing plugins...");

            foreach (IArchiveEntry plugin in _plugins)
            {
                try
                {
                    ProcessPlugin(plugin);
                }
                catch (Exception e)
                {
                    AddLogMessage("Failed to analyze plugin.");
                    AddLogMessage("Exception:" + e.Message);
                }
            }
        }

        // TODO: BackgroundWorker here
        private void AnalyzeAssetArchive(IArchiveEntry assetArchiveEntry)
        {
            assetArchiveEntry.WriteToDirectory(@".\bsas", ExtractOptions.Overwrite);

            PostProgressMessage(assetArchiveEntry.GetEntryExtesion() + " extracted, analyzing entries...");

            string assetArchivePath = Path.Combine("bsas", assetArchiveEntry.Key);

            string[] assets;

            if (assetArchiveEntry.GetEntryExtesion().Equals(".bsa", StringComparison.InvariantCultureIgnoreCase))
                assets = GetBSAAssets(assetArchivePath);
            else
                assets = GetBA2Assets(assetArchivePath);

            List<string> assetEntryPaths = assets.Select(asset => Path.Combine(assetArchiveEntry.Key, asset)).ToList();
            _modAnalysis.assets.AddRange(assetEntryPaths);
            AddLogMessages(assetEntryPaths);
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

        private void ProcessPlugin(IArchiveEntry entry)
        {
            try
            {
                ExtractPlugin(entry);
                AnalyzePlugin(entry);
                RevertPlugin(entry);
            }
            catch (Exception)
            {
                throw;
            }
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
                AddLogMessage("Failed to revert plugin!");
                AddLogMessage("!!! Please manually revert " + Path.GetFileName(entry.Key) + "!!!"); // TODO: show dialog message
                AddLogMessage("Exception:" + e.Message);
            }
        }

        public void ExtractPlugin(IArchiveEntry entry)
        {
            string gameDataPath = GameService.GetGamePath(_game);
            string pluginFileName = Path.GetFileName(entry.Key);
            string pluginFilePath = gameDataPath + pluginFileName; // TODO: use Path.Combine
            bool alreadyExtracted = _extractedPlugins.Contains(pluginFilePath);
            
            if (!alreadyExtracted)
            {
                _extractedPlugins.Add(pluginFilePath);

                if (File.Exists(pluginFilePath) && !File.Exists(pluginFilePath + ".bak"))
                    File.Move(pluginFilePath, pluginFilePath + ".bak");
            }
            
            entry.WriteToDirectory(gameDataPath, ExtractOptions.Overwrite);
        }
        
        public void AnalyzePlugin(IArchiveEntry entry)
        {
            // prepare mod dump and message buffer
            StringBuilder message = new StringBuilder(4 * 1024 * 1024);

            // prepare plugin file for dumping
            if (!ModDump.Prepare(Path.GetFileName(entry.Key)))
            {
                ModDump.GetBuffer(message, message.Capacity);
                AddLogMessage(message.ToString());
                return;
            }

            // dump the plugin file
            StringBuilder json = new StringBuilder(4 * 1024 * 1024); // 4MB maximum dump size
            if (!ModDump.Dump(json, json.Capacity))
            {
                ModDump.GetBuffer(message, message.Capacity);
                AddLogMessage(message.ToString());
                return;
            }

            PluginDump pluginDump = JsonConvert.DeserializeObject<PluginDump>(json.ToString());
            _modAnalysis.plugins.Add(pluginDump);

            // log the results
            // TODO: This should be handled better.
            ModDump.GetBuffer(message, message.Capacity);
            AddLogMessage(message.ToString());
            ModDump.FlushBuffer();
        }
    }
}
