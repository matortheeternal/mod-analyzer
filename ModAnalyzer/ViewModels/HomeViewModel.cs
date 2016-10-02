using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using System.Collections.Generic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class HomeViewModel : ViewModelBase {
        public ICommand BrowseCommand { get; set; }
        private readonly string[] archiveExts = { ".zip", ".7z", ".rar" };

        private bool _isDialogOpen;

        public bool IsDialogOpen {
            get { return _isDialogOpen; }
            set { Set(nameof(IsDialogOpen), ref _isDialogOpen, value); }
        }

        public HomeViewModel() {
            BrowseCommand = new RelayCommand(Browse);
        }

        private void Browse() {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Title = "Select a mod archive",
                Filter = "Archive Files (*.zip, *.7z, *.rar)|*.zip;*.7z;*.rar",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
                AnalyzeArchives(openFileDialog.FileNames);
        }

        public void AnalyzeArchives(string[] fileNames) {
            List<string> archiveFileNames = fileNames.ToList().FindAll(fileName => archiveExts.Contains(Path.GetExtension(fileName)));
            if (archiveFileNames.Count > 0) {
                MessengerInstance.Send(new FilesSelectedMessage(archiveFileNames));
            }
        }
    }
}
