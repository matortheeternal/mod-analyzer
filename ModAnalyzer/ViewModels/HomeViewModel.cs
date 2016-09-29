using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Messages;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class HomeViewModel : ViewModelBase {
        public ICommand BrowseCommand { get; set; }

        public HomeViewModel() {
            BrowseCommand = new RelayCommand(Browse);
        }

        private void SendFilesSelectedMessage(FilesSelectedMessage msg) {
            try {
                MessengerInstance.Send(msg);
            }
            catch {
                try {
                    Thread.Sleep(100);
                    MessengerInstance.Send(msg);
                }
                catch (Exception e) {
                    string errorMessage = (e.InnerException != null) ? e.InnerException.Message : e.Message;
                    MessageBox.Show("Error sending FilesSelectedMessage: " + errorMessage);
                }
            }
        }

        private void Browse() {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Title = "Select a mod archive",
                Filter = "Archive Files (*.zip, *.7z, *.rar)|*.zip;*.7z;*.rar",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                FilesSelectedMessage msg = new FilesSelectedMessage(openFileDialog.FileNames.ToList());
                SendFilesSelectedMessage(msg);
            }
        }
    }
}
