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
                    FilesSelectedMessage msg = new FilesSelectedMessage(openFileDialog.FileNames.ToList());
                    if (MessengerInstance != null) {
                        MessengerInstance.Send(msg);
                    }
                }
                catch (Exception e) {
                    string errorMessage = (e.InnerException != null) ? e.InnerException.Message : e.Message;
                    MessageBox.Show("Error sending FilesSelectedMessage: " + errorMessage);
                }
            }
        }
    }
}
