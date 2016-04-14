using Microsoft.Win32;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.IO;
using System.Windows;
using System.Reflection;
using BA2Lib;

namespace ModAssetMapper {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        BA2NET ba2 = new BA2NET();

        public MainWindow() {
            InitializeComponent();
        }

        public void HandleBSA(IArchiveEntry entry) {
            entry.WriteToDirectory(@".\\bsas", ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bsaPath = rootPath + "\\bsas\\" + entry.Key;
            if (ba2.Open(bsaPath)) {
                string[] entries = ba2.GetNameTable();
                for (int i = 0; i < entries.Length; i++) {
                    textBlock.Inlines.Add(entry.Key + "\\" + entries[i] + "\n");
                }
            }
        }

        public void GetEntryMap(string path) {
            var archive = ArchiveFactory.Open(@path);
            foreach (var entry in archive.Entries) {
                if (!entry.IsDirectory) {
                    string entryPath = entry.Key;
                    textBlock.Inlines.Add(entryPath + "\n");
                    string ext = Path.GetExtension(entryPath);
                    if ((String.Equals(ext, ".ba2", StringComparison.OrdinalIgnoreCase)) || 
                        (String.Equals(ext, ".bsa", StringComparison.OrdinalIgnoreCase))) {
                        HandleBSA(entry);
                    }
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                textBox.Text = openFileDialog.FileName;
                textBlock.Text = "";
                GetEntryMap(openFileDialog.FileName);
            }
        }
    }
}
