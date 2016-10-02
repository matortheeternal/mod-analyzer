using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class HomeViewModel : ViewModelBase {
        public ICommand BrowseCommand { get; set; }

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

            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            if (openFileDialog.FileNames.Count() > 1) {
                MessengerInstance.Send(new FilesSelectedMessage(openFileDialog.FileNames.ToList()));
                IsDialogOpen = true;
            } else {
                MessengerInstance.Send(new ModOptionsSelectedMessage(new List<ModOption> { new ModOption(openFileDialog.FileName, true, false) }));
            }
        }
    }
}
