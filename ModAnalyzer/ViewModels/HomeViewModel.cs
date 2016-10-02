using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Messages;
using ModAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class HomeViewModel : ViewModelBase {
        private readonly string[] archiveExts = { ".zip", ".7z", ".rar" };

        public ICommand BrowseCommand { get; set; }
        public ICommand UpdateCommand { get; set; }

        private bool _isUpdateAvailable;

        public bool IsUpdateAvailable {
            get { return _isUpdateAvailable; }
            set { Set(nameof(IsUpdateAvailable), ref _isUpdateAvailable, value); }
        }

        public HomeViewModel() {
            BrowseCommand = new RelayCommand(Browse);
            UpdateCommand = new RelayCommand(OpenDownloadPage);

            CheckForUpdate();
        }

        private void OpenDownloadPage() {
            try {
                Process.Start("https://github.com/matortheeternal/mod-analyzer/releases");
            } catch (Exception exception) {
                MessageBox.Show("Failed to open download page. Please check https://github.com/matortheeternal/mod-analyzer/releases for updates.\n\n" 
                    + exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private async void CheckForUpdate() {
            try {
                IsUpdateAvailable = await UpdateUtil.IsUpdateAvailable();
            } catch (Exception exception) {
                IsUpdateAvailable = false;
                
                MessageBox.Show("Failed to check for updates. If this error persists, please check " +
                    "https://github.com/matortheeternal/mod-analyzer/releases for updates.\n\n" + exception.ToString());
            }
        }
    }
}
