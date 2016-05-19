using BA2Lib;
using libbsa;
using Microsoft.Win32;
using Newtonsoft.Json;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace ModAnalyzer
{
    public class PluginRecordGroup {
        public string sig { get; set; }
        public int record_count { get; set; }
        public int override_count { get; set; }
    }

    public class OverrideRecord {
        public int fid { get; set; }
        public string sig { get; set; }
    }

    public class PluginError {
        public int group { get; set; }
        public string signature { get; set; }
        public int form_id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string data { get; set; }
    }

    public class PluginDump {
        public string filename { get; set; }
        public string description { get; set; }
        public string author { get; set; }
        public string crc_hash { get; set; }
        public string file_size { get; set; }
        public string record_count { get; set; }
        public string override_count { get; set; }
        public List<String> master_filenames { get; set; }
        public List<String> dummy_masters { get; set; }
        public List<PluginRecordGroup> plugin_record_groups { get; set; }
        public List<OverrideRecord> overrides { get; set; }
        public List<PluginError> plugin_errors { get; set; }
        public PluginDump() {
            master_filenames = new List<string>();
            dummy_masters = new List<string>();
            plugin_record_groups = new List<PluginRecordGroup>();
            overrides = new List<OverrideRecord>();
            plugin_errors = new List<PluginError>();
        }
    }

    public class ModAnalysis {
        public List<String> assets { get; set; }
        public List<PluginDump> plugins { get; set; }
        public ModAnalysis() {
            assets = new List<string>();
            plugins = new List<PluginDump>();
        }
    }

    public static class ModDump {
        /// <summary>
        /// Linking Delphi DLL for dumping plugins
        /// </summary>
        [DllImport(@"ModDumpLib.dll")]
        public static extern void StartModDump();
        [DllImport(@"ModDumpLib.dll")]
        public static extern void EndModDump();
        [DllImport(@"ModDumpLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void GetBuffer(StringBuilder str, int len);
        [DllImport(@"ModDumpLib.dll")]
        public static extern void FlushBuffer();
        [DllImport(@"ModDumpLib.dll")]
        public static extern void SetGameMode(int mode);
        [DllImport(@"ModDumpLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Prepare(string FilePath);
        [DllImport(@"ModDumpLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Dump(StringBuilder str, int len);
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        // link to BA2Net
        // link to BSANet
        BA2NET ba2 = new BA2NET();
        BSANET bsa = new BSANET();
        ModAnalysis analysis = new ModAnalysis();
        int visibility_mode = 0;
        int gameMode = 1;
        string DataPath;

        public MainWindow() {
            InitializeComponent();
            ModDump.StartModDump();
            GetDataPath();
        }

        ~MainWindow() {
            ba2.Dispose();
            bsa.bsa_close();
            ModDump.EndModDump();
        }

        public void GetDataPath() {
            DataPath = "C:\\SteamLibrary\\steamapps\\common\\Skyrim\\data\\";
        }

        public void HandleBA2(IArchiveEntry entry) {
            entry.WriteToDirectory(@".\\bsas", ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
            ProgressMessage("BSA extracted, Analyzing entries...");
            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bsaPath = rootPath + "\\bsas\\" + entry.Key;
            if (ba2.Open(bsaPath)) {
                string[] entries = ba2.GetNameTable();
                for (int i = 0; i < entries.Length; i++) {
                    String entryPath = entry.Key + "\\" + entries[i];
                    LogMessage(entryPath);
                    analysis.assets.Add(entryPath);
                }
            }
        }

        public void HandleBSA(IArchiveEntry entry) {
            entry.WriteToDirectory(@".\\bsas", ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bsaPath = rootPath + "\\bsas\\" + entry.Key;
            if (bsa.bsa_open(bsaPath) == 0) {
                string[] entries = bsa.bsa_get_assets(".*");
                for (int i = 0; i < entries.Length; i++) {
                    String entryPath = entry.Key + "\\" + entries[i];
                    LogMessage(entryPath);
                    analysis.assets.Add(entryPath);
                }
            }
        }

        public void HandlePlugin(IArchiveEntry entry) {
            string pluginPath = DataPath + entry.Key;
            bool deleteAfter = false;
            if (!File.Exists(pluginPath)) {
                deleteAfter = true;
                entry.WriteToDirectory(DataPath, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
            }
            //TODO: This should be dynamic
            ModDump.SetGameMode(gameMode);
            StringBuilder msg = new StringBuilder(4 * 1024 * 1024);

            // prepare plugin file for dumping
            if (!ModDump.Prepare(pluginPath)) {
                ModDump.GetBuffer(msg, msg.Capacity);
                LogMessage(msg.ToString());
                return;
            }

            // dump the plugin file
            int maxDumpSize = 4 * 1024 * 1024; // 4MB maximum dump size
            StringBuilder json = new StringBuilder(maxDumpSize);
            if (!ModDump.Dump(json, maxDumpSize)) {
                ModDump.GetBuffer(msg, msg.Capacity);
                LogMessage(msg.ToString());
                return;
            }

            // deserialize the json
            PluginDump pd = JsonConvert.DeserializeObject<PluginDump>(json.ToString());
            analysis.plugins.Add(pd);

            // log the results
            // TODO: This should be handled better.
            ModDump.GetBuffer(msg, msg.Capacity);
            LogMessage(msg.ToString());

            if (deleteAfter) {
                File.Delete(pluginPath);
            }
        }

        public void LogMessage(string message) {
            //if (textBlock.Inlines.Count == 0) {
                textBlock.Inlines.Add(message + "\n");
            //}
            //else {
            //    textBlock.Inlines.InsertBefore(textBlock.Inlines.FirstInline, new Run(message + "\n"));
            //}
        }

        public void ProgressMessage(string message) {
            loading_label.Content = message;
        }

        public void GetEntryMap(string path) {
            // load the archive
            var archive = ArchiveFactory.Open(@path);
            ProgressMessage("Analyzing archive entries...");
            // loop through entries in archive
            foreach (var entry in archive.Entries) {
                // for non-directory entries, store the 
                if (!entry.IsDirectory) {
                    string entryPath = entry.Key.Replace('/', '\\');
                    analysis.assets.Add(entryPath);
                    LogMessage(entryPath);
                    string ext = Path.GetExtension(entryPath);
                    if (String.Equals(ext, ".ba2", StringComparison.OrdinalIgnoreCase)) {
                        ProgressMessage("Extracting BA2 at " + entryPath);
                        HandleBA2(entry);
                        ProgressMessage("Analyzing archive entries...");
                    } else if (String.Equals(ext, ".bsa", StringComparison.OrdinalIgnoreCase)) {
                        ProgressMessage("Extracting BSA at " + entryPath);
                        HandleBSA(entry);
                        ProgressMessage("Analyzing archive entries...");
                    } else if (String.Equals(ext, ".esp", StringComparison.OrdinalIgnoreCase)) {
                        ProgressMessage("Extracting ESP at " + entryPath);
                        HandlePlugin(entry);
                        ProgressMessage("Analyzing plugin file...");
                    } else if (String.Equals(ext, ".esm", StringComparison.OrdinalIgnoreCase)) {
                        ProgressMessage("Extracting ESM at " + entryPath);
                        HandlePlugin(entry);
                        ProgressMessage("Analyzing plugin file...");
                    }
                }
            }

            // add padding to bottom of text block
            textBlock.Inlines.Add("\n\n\n");
        }

        private void toggleControlVisibility() {
            // determine visibility modes
            Visibility v1 = visibility_mode == 0 ? Visibility.Hidden : Visibility.Visible;
            Visibility v2 = visibility_mode == 0 ? Visibility.Visible : Visibility.Hidden;

            // toggle visibility of group 1
            prompt_label.Visibility = v1;
            logo.Visibility = v1;
            browse_button.Visibility = v1;

            // toggle visibility of group 2
            loading_label.Visibility = v2;
            scrollviewer.Visibility = v2;
            reset_button.Visibility = v2;

            // toggle mode
            visibility_mode = (visibility_mode + 1) % 2;
        }

        private void browse_button_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                // toggle control visibility
                toggleControlVisibility();

                // set filename
                ProgressMessage("Loading " + openFileDialog.FileName + "...");

                // reset listing to empty
                textBlock.Text = "";

                // get the entry file tree
                GetEntryMap(openFileDialog.FileName);

                // write entry file tree to json file
                string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                String filename = rootPath + "\\" + Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                loading_label.Content = "Saving JSON to " + filename + ".json...";
                File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(analysis));
                loading_label.Content = "All done.  JSON file saved to "+ filename + ".json";
            }
        }

        private void reset_button_Click(object sender, RoutedEventArgs e) {
            analysis.assets.Clear();
            analysis.plugins.Clear();
            textBlock.Text = "";
            toggleControlVisibility();
        }
    }
}
