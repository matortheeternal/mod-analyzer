using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using ModAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class HomeViewModel : ViewModelBase {
        private readonly string[] archiveExts = { ".zip", ".7z", ".rar" };

        public ICommand BrowseCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public Visibility LockVisibility { get; set; } = Visibility.Collapsed;
        public int SelectedGameIndex { get; set; } = 3; // default to Skyrim game mode
        public bool CanChangeGameMode { get; set; } = true;
        public string GameToolTip { get; set; } = 
            "You must restart the program to change the" + Environment.NewLine + 
            "game mode after analyzing a plugin file.";

        private bool _isUpdateAvailable;

        public bool IsUpdateAvailable {
            get { return _isUpdateAvailable; }
            set { Set(nameof(IsUpdateAvailable), ref _isUpdateAvailable, value); }
        }

        private bool _isDialogOpen;

        public bool IsDialogOpen {
            get { return _isDialogOpen; }
            set { Set(nameof(IsDialogOpen), ref _isDialogOpen, value); }
        }

        public HomeViewModel() {
            // set up command relays
            BrowseCommand = new RelayCommand(Browse);
            UpdateCommand = new RelayCommand(OpenDownloadPage);

            // initialize game combobox and check for program updates
            CheckForUpdate();

            // set up message listeners
            MessengerInstance.Register<AnalysisCompleteMessage>(this, OnAnalysisComplete);
        }

        private void OnAnalysisComplete(AnalysisCompleteMessage message) {
            UpdateGameComboBox();
        }

        public void UpdateGameComboBox() {
            if (GameService.currentGame != null) {
                SelectedGameIndex = GameService.currentGame.gameMode;
            }
            CanChangeGameMode = !ModDump.started;
            LockVisibility = CanChangeGameMode ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OpenDownloadPage() {
            string url = "https://github.com/matortheeternal/mod-analyzer/releases";
            try {
                Process.Start("url");
            } catch (Exception exception) {
                string errorMessage = "Failed to open download page. Please check " + url + "for updates.\n\n" + exception.ToString();
                System.Windows.Forms.MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Browse() {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Title = "Select a mod archive",
                Filter = "Archive Files (*.zip, *.7z, *.rar)|*.zip;*.7z;*.rar",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                AnalyzeArchives(openFileDialog.FileNames);
            }
        }

        public void AnalyzeArchives(string[] fileNames) {
            List<string> validArchives = fileNames.Where(fileName => archiveExts.Contains(Path.GetExtension(fileName))).ToList();
            if (validArchives.Count == 0) return;

            MessengerInstance.Send(new FilesSelectedMessage(validArchives));
            IsDialogOpen = true;
        }

        private async void CheckForUpdate() {
            try {
                IsUpdateAvailable = await UpdateUtil.IsUpdateAvailable();
            } catch (Exception exception) {
                IsUpdateAvailable = false;
                string errorMessage = "Failed to check for updates. If this error persists, please check " +
                    "https://github.com/matortheeternal/mod-analyzer/releases for updates.\n\n" + exception.ToString();
                System.Windows.Forms.MessageBox.Show(errorMessage);
            }
        }
    }
}
