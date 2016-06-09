using BA2Lib;
using GalaSoft.MvvmLight;
using libbsa;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using Newtonsoft.Json;
using SharpCompress.Archive;
using SharpCompress.Common;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace ModAnalyzer.ViewModels
{
    public class AnalysisViewModel : ViewModelBase
    {
        private readonly BA2NET _ba2Manager;
        private readonly BSANET _bsaManager;
        private ModAnalysis _modAnalysis;

        public ObservableCollection<string> LogMessages { get; set; }

        public AnalysisViewModel()
        {
            _ba2Manager = new BA2NET();
            _bsaManager = new BSANET();
            _modAnalysis = new ModAnalysis();

            // set game mode to Skyrim
            // TODO: Make this dynamic from GUI
            GameService.game = GameService.getGame("Skyrim");
            ModDump.SetGameMode(GameService.game.gameMode);

            LogMessages = new ObservableCollection<string>();

            MessengerInstance.Register<FileSelectedMessage>(this, OnFileSelectedMessage);
        }

        private void SendProgressMessage(string message)
        {
            MessengerInstance.Send(new ProgressMessage(message));
        }

        private void OnFileSelectedMessage(FileSelectedMessage message)
        {
            LogMessages.Clear();

            SendProgressMessage("Loading " + message.FilePath + "...");

            GetEntryMap(message.FilePath);

            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filename = Path.Combine(rootPath, Path.GetFileNameWithoutExtension(message.FilePath));

            SendProgressMessage("Saving JSON to " + filename + ".json...");
            File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(_modAnalysis));
            SendProgressMessage("All done.  JSON file saved to " + filename + ".json");
        }

        ~AnalysisViewModel()
        {
            _ba2Manager.Dispose();
            _bsaManager.bsa_close();
        }

        private void GetEntryMap(string path)
        {
            IArchive archive = ArchiveFactory.Open(@path);

            SendProgressMessage("Analyzing archive entries...");

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
                        SendProgressMessage("Extracting BA2 at " + entryPath);
                        HandleBA2(entry);
                        break;
                    case ".BSA":
                        SendProgressMessage("Extracting BSA at " + entryPath);
                        HandleBSA(entry);
                        break;
                    case ".ESP":
                    case ".ESM":
                        SendProgressMessage("Analyzing plugin file...");
                        try {
                            HandlePlugin(entry);
                        } catch (System.Exception e) {
                            LogMessages.Add("Failed to analyze plugin.");
                            LogMessages.Add("Exception:"+e.Message);
                        }
                        break;
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

        public void HandlePlugin(IArchiveEntry entry)
        {
            // prepare mod dump and message buffer
            ModDump.StartModDump();
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
            
            // Finalize ModDump
            ModDump.EndModDump();
        }
    }
}
