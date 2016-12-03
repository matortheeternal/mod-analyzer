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
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class ClassifyArchivesViewModel : ViewModelBase {
        public ObservableCollection<ModOption> ArchiveModOptions { get; set; }
        public VisibilityConverter VisibilityConverter1 { get; set; }
        public ICommand AnalyzeCommand { get; set; }

        public ClassifyArchivesViewModel() {
            ArchiveModOptions = new ObservableCollection<ModOption>();
            AnalyzeCommand = new RelayCommand(AnalyzeMod);
            VisibilityConverter1 = new VisibilityConverter();

            MessengerInstance.Register<FilesSelectedMessage>(this, OnFilesSelected);
        }
                
        private void OnFilesSelected(FilesSelectedMessage message) {
            ArchiveModOptions.Clear();
            bool oneFilePath = message.FilePaths.Count == 1;

            foreach (string filePath in message.FilePaths) {
                string fileName = Path.GetFileName(filePath);
                ArchiveModOptions.Add(new ModOption(fileName, filePath, oneFilePath));
            }
        }

        private void AnalyzeMod() {
            MessengerInstance.Send(new ArchiveModOptionsSelectedMessage(ArchiveModOptions.ToList()));
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
