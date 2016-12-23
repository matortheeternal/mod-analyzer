using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class ClassifyArchivesViewModel : ViewModelBase {
        public ObservableCollection<ModOption> ArchiveModOptions { get; set; }
        public VisibilityConverter VisibilityConverter1 { get; set; }
        public ICommand AnalyzeCommand { get; set; }
        public ICommand AddArchiveCommand { get; set; }

        public ClassifyArchivesViewModel() {
            ArchiveModOptions = new ObservableCollection<ModOption>();
            AnalyzeCommand = new RelayCommand(AnalyzeMod);
            AddArchiveCommand = new RelayCommand(AddArchive);
            VisibilityConverter1 = new VisibilityConverter();

            MessengerInstance.Register<FilesSelectedMessage>(this, OnFilesSelected);
        }

        private bool ArchiveModOptionExists(string filePath) {
            ModOption existingArchiveOption = ArchiveModOptions.Where(
                x => x.SourceFilePath == filePath
            ).FirstOrDefault();
            return existingArchiveOption != null;
        }
                
        private void OnFilesSelected(FilesSelectedMessage message) {
            if (message.ReplaceSelection) ArchiveModOptions.Clear();
            bool oneFilePath = message.FilePaths.Count == 1;

            foreach (string filePath in message.FilePaths) {
                string fileName = Path.GetFileName(filePath);
                if (ArchiveModOptionExists(filePath)) continue;
                ArchiveModOptions.Add(new ModOption(fileName, filePath, oneFilePath));
            }
        }

        public void Back() {
            MessengerInstance.Send(new NavigationMessage(Page.Home));
        }

        private void AnalyzeMod() {
            MessengerInstance.Send(new ArchiveModOptionsSelectedMessage(ArchiveModOptions.ToList()));
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

    public class VisibilityConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            bool visibility = (bool)value;
            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            Visibility visibility = (Visibility)value;
            return (visibility == Visibility.Visible);
        }
    }
}
