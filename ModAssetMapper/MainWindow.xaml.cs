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
        AssetFileList list;

        public MainWindow() {
            InitializeComponent();
            list = new AssetFileList();
        }

        public void HandleBA2(IArchiveEntry entry) {
            entry.WriteToDirectory(@".\\bsas", ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bsaPath = rootPath + "\\bsas\\" + entry.Key;
            if (ba2.Open(bsaPath)) {
                string[] entries = ba2.GetNameTable();
                for (int i = 0; i < entries.Length; i++) {
                    String entryPath = entry.Key + "\\" + entries[i];
                    textBlock.Inlines.Add(entryPath + "\n");
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
                    textBlock.Inlines.Add(entryPath + "\n");
                    list.assets.Add(entryPath);
                }
            }
        }

        public void GetEntryMap(string path) {
            // load the archive
            var archive = ArchiveFactory.Open(@path);
            // loop through entries in archive
            foreach (var entry in archive.Entries) {
                // for non-directory entries, store the 
                if (!entry.IsDirectory) {
                    string entryPath = entry.Key;
                    list.assets.Add(entryPath);
                    textBlock.Inlines.Add(entryPath + "\n");
                    string ext = Path.GetExtension(entryPath);
                    if (String.Equals(ext, ".ba2", StringComparison.OrdinalIgnoreCase)) {
                        HandleBA2(entry);
                    } else if (String.Equals(ext, ".bsa", StringComparison.OrdinalIgnoreCase)) {
                        HandleBSA(entry);
                    }
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                // set filename
                textBox.Text = openFileDialog.FileName;

                // reset listing to empty
                textBlock.Text = "";

                // get the entry file tree
                GetEntryMap(openFileDialog.FileName);

                // write entry file tree to json file
                String filename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                File.WriteAllText(@".\" + filename + ".json", JsonConvert.SerializeObject(list));
            }
        }
    }
}
