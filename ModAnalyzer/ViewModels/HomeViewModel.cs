using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Messages;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class HomeViewModel : ViewModelBase {
        public ICommand BrowseCommand { get; set; }

        public HomeViewModel() {
            BrowseCommand = new RelayCommand(Browse);
        }

        private void Browse() {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Title = "Select a mod archive",
                Filter = "Archive Files (*.zip, *.7z, *.rar)|*.zip;*.7z;*.rar",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                try {
                    MessengerInstance.Send(new FilesSelectedMessage(openFileDialog.FileNames.ToList()));
                }
                catch (Exception e) {
                    string errorMessage;
                    if (e.InnerException != null) {
                        errorMessage = e.InnerException.Message;
                    }
                    else {
                        errorMessage = e.Message;
                    }
                    MessageBox.Show("Error sending FilesSelectedMessage: " + errorMessage);
                }
            }
        }
    }
}
