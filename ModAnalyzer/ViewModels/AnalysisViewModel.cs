using BA2Lib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using libbsa;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using ModAssetMapper;
using Newtonsoft.Json;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels
{
    public class AnalysisViewModel : ViewModelBase
    {
        private readonly BA2NET _ba2Manager;
        private readonly BSANET _bsaManager;
        private ModAnalysis _modAnalysis;
        private List<string> extracted;
        private List<IArchiveEntry> plugins;
        private BackgroundWorker _backgroundWorker;
        private BackgroundWorker _backgroundWorker2;

        public ICommand ResetCommand { get; set; }
        public ICommand ViewOutputCommand { get; set; }
        public ObservableCollection<string> LogMessages { get; set; }

        public AnalysisViewModel()
        {
            _ba2Manager = new BA2NET();
            _bsaManager = new BSANET();
            _modAnalysis = new ModAnalysis();
            extracted = new List<string>();
            plugins = new List<IArchiveEntry>();

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;

            _backgroundWorker2 = new BackgroundWorker();
            _backgroundWorker2.DoWork += _backgroundWorker2_DoWork;
            _backgroundWorker2.WorkerReportsProgress = true;
            _backgroundWorker2.ProgressChanged += _backgroundWorker2_ProgressChanged;

            // set game mode to Skyrim
            // TODO: Make this dynamic from GUI
            ModDump.StartModDump();
            GameService.game = GameService.getGame("Skyrim");
            ModDump.SetGameMode(GameService.game.gameMode);

            ResetCommand = new RelayCommand(() => MessengerInstance.Send(new NavigationMessage(Page.Home)));
            ViewOutputCommand = new RelayCommand(() => Process.Start("output"));
            LogMessages = new ObservableCollection<string>();

            MessengerInstance.Register<FileSelectedMessage>(this, OnFileSelectedMessage);
        }

        private void _backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate { LogMessages.Add(e.UserState.ToString()); });
            
        }

        private void _backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;

            IArchive archive = ArchiveFactory.Open(e.Argument.ToString());

            SendProgressMessage("Analyzing archive entries...");

            // loop through archive entries
            foreach (IArchiveEntry entry in archive.Entries)
            {
                if (entry.IsDirectory)
                    continue;

                string entryPath = entry.Key.Replace('/', '\\');
                _modAnalysis.assets.Add(entryPath);

                backgroundWorker.ReportProgress(0, entryPath);

                string extension = Path.GetExtension(entryPath).ToUpper();

                switch (extension)
                {
                    case ".BA2":
                        SendProgressMessage("Extracting BA2 at " + entryPath);
                        HandleBA2(entry);
                        break;
                    case ".BSA":
                        SendProgressMessage("Extracting BSA at " + entryPath);
                        HandleBSA(entry);
                        break;
                    case ".ESP":
                    case ".ESM":
                        ExtractPlugin(entry);
                        plugins.Add(entry);
                        break;
                }
            }
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LogMessages.Add(e.UserState.ToString());
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.ReportProgress(0,"Processing wave 1");

            if (!Directory.Exists("output"))
                Directory.CreateDirectory("output");

            string filePath = e.Argument.ToString();

            LogMessages.Clear();

            SendProgressMessage("Loading " + filePath + "...");

            try
            {
                _backgroundWorker2.RunWorkerAsync(filePath);

                if (plugins.Count > 0)
                {
                    HandlePlugins();
                    RevertPlugins();
                }
            }
            catch (Exception exception)
            {
                LogMessages.Add("Failed to analyze archive.");
                LogMessages.Add("Exception:" + exception.Message);
            }

            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filename = Path.Combine(rootPath, "output", Path.GetFileNameWithoutExtension(filePath));

            SendProgressMessage("Saving JSON to " + filename + ".json...");
            File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(_modAnalysis));
            SendProgressMessage("All done.  JSON file saved to " + filename + ".json");
        }

        private void SendProgressMessage(string message)
        {
            MessengerInstance.Send(new ProgressMessage(message));
        }

        private void OnFileSelectedMessage(FileSelectedMessage message)
        {
            _backgroundWorker.RunWorkerAsync(message.FilePath);
        }

        ~AnalysisViewModel()
        {
            _ba2Manager.Dispose();
            _bsaManager.bsa_close();
        }

        public void HandlePlugins()
        {
            LogMessages.Add("Extracting and analyzing plugins...");
            foreach (IArchiveEntry entry in plugins)
            {
                try
                {
                    ExtractPlugin(entry);
                    HandlePlugin(entry);
                }
                catch (System.Exception e)
                {
                    LogMessages.Add("Failed to analyze plugin.");
                    LogMessages.Add("Exception:" + e.Message);
                }
            }
        }

        public void RevertPlugins()
        {
            LogMessages.Add("Restoring plugins...");
            foreach (IArchiveEntry entry in plugins)
            {
                try
                {
                    RevertPlugin(entry);
                }
                catch (System.Exception e)
                {
                    LogMessages.Add("Failed to revert plugin!");
                    LogMessages.Add("!!! Please manually revert " + Path.GetFileName(entry.Key) + "!!!");
                    LogMessages.Add("Exception:" + e.Message);
                }
            }
        }

        public void HandleBA2(IArchiveEntry entry)
        {
            Directory.CreateDirectory(@".\bsas");
            entry.WriteToDirectory(@".\bsas", ExtractOptions.Overwrite);

            SendProgressMessage("BA2 extracted, Analyzing entries...");

            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string ba2Path = Path.Combine(rootPath, "bsas", entry.Key);

            if (_ba2Manager.Open(ba2Path))
            {
                string[] entries = _ba2Manager.GetNameTable();

                for (int i = 0; i < entries.Length; i++)
                {
                    string entryPath = Path.Combine(entry.Key, entries[i]);
                    _modAnalysis.assets.Add(entryPath);
                    LogMessages.Add(entryPath);
                }
            }
        }

        public void HandleBSA(IArchiveEntry entry)
        {
            Directory.CreateDirectory(@".\bsas");
            entry.WriteToDirectory(@".\bsas\", ExtractOptions.Overwrite);

            SendProgressMessage("BSA extracted, Analyzing entries...");

            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bsaPath = Path.Combine(rootPath, "bsas", entry.Key);

            if (_bsaManager.bsa_open(bsaPath) == 0)
            {
                string[] entries = _bsaManager.bsa_get_assets(".*");
                for (int i = 0; i < entries.Length; i++)
                {
                    string entryPath = Path.Combine(entry.Key, entries[i]);
                    _modAnalysis.assets.Add(entryPath);
                    LogMessages.Add(entryPath);
                }
            }
        }

        public void RevertPlugin(IArchiveEntry entry)
        {
            string dataPath = GameService.getDataPath();
            string filename = Path.GetFileName(entry.Key);
            string filepath = dataPath + filename;

            File.Delete(filepath);
            if (File.Exists(filepath + ".bak"))
            {
                File.Move(filepath + ".bak", filepath);
            }
        }

        public void ExtractPlugin(IArchiveEntry entry)
        {
            string dataPath = GameService.getDataPath();
            string filename = Path.GetFileName(entry.Key);
            string filepath = dataPath + filename;
            bool alreadyExtracted = extracted.IndexOf(filepath) > -1;

            // track extraction
            if (!alreadyExtracted)
            {
                extracted.Add(filepath);
                // move existing file if present
                if (File.Exists(filepath) && !File.Exists(filepath + ".bak"))
                {
                    File.Move(filepath, filepath + ".bak");
                }
            }

            // extract file
            entry.WriteToDirectory(dataPath, ExtractOptions.Overwrite);
        }

        public void HandlePlugin(IArchiveEntry entry)
        {
            // prepare mod dump and message buffer
            StringBuilder message = new StringBuilder(4 * 1024 * 1024);

            // prepare plugin file for dumping
            if (!ModDump.Prepare(Path.GetFileName(entry.Key)))
            {
                ModDump.GetBuffer(message, message.Capacity);
                LogMessages.Add(message.ToString());
                return;
            }

            // dump the plugin file
            StringBuilder json = new StringBuilder(4 * 1024 * 1024); // 4MB maximum dump size
            if (!ModDump.Dump(json, json.Capacity))
            {
                ModDump.GetBuffer(message, message.Capacity);
                LogMessages.Add(message.ToString());
                return;
            }

            PluginDump pluginDump = JsonConvert.DeserializeObject<PluginDump>(json.ToString());
            _modAnalysis.plugins.Add(pluginDump);

            // log the results
            // TODO: This should be handled better.
            ModDump.GetBuffer(message, message.Capacity);
            LogMessages.Add(message.ToString());
            ModDump.FlushBuffer();
        }
    }
}
