using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Input;
using System;
using System.Windows.Forms;
using SharpCompress.Archive;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using SharpCompress.Common;
using System.Reflection;
using BA2Lib;
using libbsa;
using System.Text;
using Newtonsoft.Json;

namespace ModAnalyzer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly BA2NET _ba2Manager;
        private readonly BSANET _bsaManager;
        private ModAnalysis _modAnalysis;

        public ICommand BrowseCommand { get; set; }
        public ObservableCollection<string> LogMessages { get; set; }

        private string _progressMessage;

        public string ProgressMessage
        {
            get { return _progressMessage; }
            set { Set(nameof(ProgressMessage), ref _progressMessage, value); }
        }

        public MainViewModel()
        {
            _ba2Manager = new BA2NET();
            _bsaManager = new BSANET();
            _modAnalysis = new ModAnalysis();

            LogMessages = new ObservableCollection<string>();

            BrowseCommand = new RelayCommand(Browse);
        }

        ~MainViewModel()
        {
            _ba2Manager.Dispose();
            _bsaManager.bsa_close();
        }

        private void Browse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Title = "Select a mod archive", Filter = "Archive Files (*.zip, *.7z, *.rar)|*.zip;*.7z;*.rar" };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // toggle control visibility
                //toggleControlVisibility();

                LogMessages.Clear();
                
                ProgressMessage = "Loading " + openFileDialog.FileName + "...";

                // reset listing to empty
                //textBlock.Text = "";
                
                GetEntryMap(openFileDialog.FileName);

                string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                String dumpFilename = Path.Combine(rootPath, Path.GetFileNameWithoutExtension(openFileDialog.FileName)) + ".json";

                ProgressMessage = "Saving JSON to " + dumpFilename + "...";
                File.WriteAllText(dumpFilename, JsonConvert.SerializeObject(_modAnalysis));
                ProgressMessage = "All done.  JSON file saved to "+ dumpFilename;
            }
        }

        private void GetEntryMap(string path)
        {
            IArchive archive = ArchiveFactory.Open(@path);

            ProgressMessage = "Analyzing archive entries...";

            foreach (IArchiveEntry entry in archive.Entries)
            {
                if (entry.IsDirectory)
                    continue;

                string entryPath = entry.Key.Replace('/', '\\');
                _modAnalysis.assets.Add(entryPath);

                LogMessages.Add(entryPath);

                string extension = Path.GetExtension(entryPath).ToUpper();

                switch (extension)
                {
                    case ".BA2":
                        ProgressMessage = "Extracting BA2 at " + entryPath;
                        HandleBA2(entry);
                        ProgressMessage = "Analyzing archive entries...";
                        break;
                    case ".BSA":
                        ProgressMessage = "Extracting BSA at " + entryPath;
                        HandleBSA(entry);
                        ProgressMessage = "Analyzing archive entries...";
                        break;
                    case ".ESP":
                    case ".ESM":
                        ProgressMessage = "Extracting " + extension + " at " + entryPath;
                        HandlePlugin(entry);
                        ProgressMessage = "Analyzing plugin file...";
                        break;
                }
            }
        }

        public void HandleBA2(IArchiveEntry entry)
        {
            entry.WriteToDirectory(@".\\bsas", ExtractOptions.Overwrite);

            ProgressMessage = "BSA extracted, Analyzing entries...";

            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string ba2Path = Path.Combine(rootPath, @"\bsas\", Path.GetFileName(entry.Key));

            if (_ba2Manager.Open(ba2Path))
            {
                string[] entries = _ba2Manager.GetNameTable();

                for (int i = 0; i < entries.Length; i++)
                {
                    string entryPath = entry.Key + "\\" + entries[i];
                    _modAnalysis.assets.Add(entryPath);
                    LogMessages.Add(entryPath);
                }
            }
        }

        public void HandleBSA(IArchiveEntry entry)
        {
            entry.WriteToDirectory(@".\\bsas", ExtractOptions.Overwrite);

            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bsaPath = Path.Combine(rootPath, "bsas", Path.GetFileName(entry.Key));

            if (_bsaManager.bsa_open(bsaPath) == 0)
            {
                string[] entries = _bsaManager.bsa_get_assets(".*");
                for (int i = 0; i < entries.Length; i++)
                {
                    string entryPath = entry.Key + "\\" + entries[i];
                    _modAnalysis.assets.Add(entryPath);
                    LogMessages.Add(entryPath);
                }
            }
        }

        public void HandlePlugin(IArchiveEntry entry)
        {
            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(rootPath, "plugins");
            string pluginPath = Path.Combine(rootPath, "plugins", Path.GetFileName(entry.Key));

            bool deleteAfter = false;
            if (!File.Exists(pluginPath))
            {
                deleteAfter = true;
                entry.WriteToDirectory(pluginsPath, ExtractOptions.Overwrite);
            }

            ModDump.StartModDump();
            //TODO: This should be dynamic
            ModDump.SetGameMode(1);

            StringBuilder message = new StringBuilder(4 * 1024 * 1024);

            // prepare plugin file for dumping
            if (!ModDump.Prepare(pluginPath))
            {
                ModDump.GetBuffer(message, message.Capacity);
                LogMessages.Add(message.ToString());
                return;
            }

            // dump the plugin file
            int maxDumpSize = 4 * 1024 * 1024; // 4MB maximum dump size
            StringBuilder json = new StringBuilder(maxDumpSize);
            if (!ModDump.Dump(json, maxDumpSize))
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

            if (deleteAfter)
            {
                File.Delete(pluginPath);
            }

            // Finalize ModDump
            ModDump.EndModDump();
        }
    }
}