using Microsoft.Win32;
using SharpCompress.Archive;
using SharpCompress.Common;
using SharpCompress.Reader;
using System.IO;
using System.Windows;

namespace ModAssetMapper {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        public void GetEntryMap(string path) {
            var archive = ArchiveFactory.Open(@path);
            foreach (var entry in archive.Entries) {
                if (!entry.IsDirectory) {
                    string entryPath = entry.Key;
                    textBlock.Inlines.Add(entryPath + "\n");
                    string ext = Path.GetExtension(entryPath);
                    if (ext == "bsa") {
                        entry.WriteToDirectory(@".\bsas", ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                        // TODO: do bsa stuff next
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
