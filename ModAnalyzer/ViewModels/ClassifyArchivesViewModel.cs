using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Messages;
using ModAnalyzer.Analysis.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using ModAnalyzer.Domain.Services;

namespace ModAnalyzer.ViewModels {
    public class ClassifyArchivesViewModel : ViewModelBase {
        public ObservableCollection<ModOption> ArchiveModOptions { get; set; }
        public ICommand AnalyzeCommand { get; set; }
        public ICommand AddArchiveCommand { get; set; }
        public int SelectedGameIndex { get; set; } = 3; // default to Skyrim game mode

        public ClassifyArchivesViewModel() {
            ArchiveModOptions = new ObservableCollection<ModOption>();
            AnalyzeCommand = new RelayCommand(AnalyzeMod);
            AddArchiveCommand = new RelayCommand(AddArchive);

            MessengerInstance.Register<FilesSelectedMessage>(this, OnFilesSelected);
        }

        public void UpdateGameComboBox() {
            if (GameService.currentGame == null) {
                GameService.SetGame("Skyrim");
            }
            SelectedGameIndex = GameService.currentGame.gameMode;
        }

        private bool ArchiveModOptionExists(string filePath) {
            ModOption existingArchiveOption = ArchiveModOptions.Where(
                x => x.SourceFilePath == filePath
            ).FirstOrDefault();
            return existingArchiveOption != null;
        }

        private void OnFilesSelected(FilesSelectedMessage message) {
            UpdateGameComboBox();
            if (message.ReplaceSelection) ArchiveModOptions.Clear();
            bool oneFilePath = message.FilePaths.Count == 1;

            foreach (string filePath in message.FilePaths) {
                string fileName = Path.GetFileName(filePath);
                if (ArchiveModOptionExists(filePath)) continue;
                ArchiveModOptions.Add(new ModOption(fileName, filePath, oneFilePath));
            }
        }

        public void Back() {
            MessengerInstance.Send(new NavigationMessage("Home"));
        }

        private void AnalyzeMod() {
            MessengerInstance.Send(new NavigationMessage("ExtractArchives"));
            ViewModelLocator.Instance().ExtractArchivesViewModel.OnArchivesClassified(ArchiveModOptions.ToList());
        }

        private void AddArchive() {
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
            FilesSelectedMessage.SelectArchives(fileNames, MessengerInstance, false);
        }
    }
}
