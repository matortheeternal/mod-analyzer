using Microsoft.Win32;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.IO;
using System.Windows;
using System.Reflection;
using BA2Lib;
using libbsa;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Documents;
using System.ComponentModel;

namespace ModAssetMapper {

    public class AssetFileList {
        public List<String> assets { get; set; }
        public AssetFileList() {
            assets = new List<string>();
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        BA2NET ba2 = new BA2NET();
        BSANET bsa = new BSANET();
        AssetFileList list = new AssetFileList();
        int visibility_mode = 0;

        public MainWindow() {
            InitializeComponent();
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
                    list.assets.Add(entryPath);
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
                    list.assets.Add(entryPath);
                }
            }
        }

        public void LogMessage(string message) {
            if (textBlock.Inlines.Count == 0) {
                textBlock.Inlines.Add(message + "\n");
            }
            else {
                textBlock.Inlines.InsertBefore(textBlock.Inlines.FirstInline, new Run(message + "\n"));
            }
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
                    string entryPath = entry.Key;
                    list.assets.Add(entryPath);
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
                String filename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                loading_label.Content = "Saving JSON to " + filename + ".json...";
                File.WriteAllText(@".\" + filename + ".json", JsonConvert.SerializeObject(list));
                loading_label.Content = "All done.  JSON file saved to "+ filename + ".json";
            }
        }

        private void reset_button_Click(object sender, RoutedEventArgs e) {
            list.assets.Clear();
            textBlock.Text = "";
            toggleControlVisibility();
        }
    }
}
