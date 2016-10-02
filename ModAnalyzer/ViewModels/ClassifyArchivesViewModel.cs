using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class ClassifyArchivesViewModel : ViewModelBase {
        public ObservableCollection<ModOption> ArchiveModOptions { get; set; }
        public ICommand AnalyzeCommand { get; set; }

        public ClassifyArchivesViewModel() {
            ArchiveModOptions = new ObservableCollection<ModOption>();
            AnalyzeCommand = new RelayCommand(() => MessengerInstance.Send(new ArchiveModOptionsSelectedMessage(ArchiveModOptions.ToList())));

            MessengerInstance.Register<FilesSelectedMessage>(this, OnFilesSelected);
        }

        private void OnFilesSelected(FilesSelectedMessage message) {
            ArchiveModOptions.Clear();

            foreach (string file in message.FilePaths)
                ArchiveModOptions.Add(new ModOption(Path.GetFileName(file), false, false) { SourceFilePath = file });

            if (message.FilePaths.Count == 1) {
                ArchiveModOptions.First().Default = true;
                MessengerInstance.Send(new ArchiveModOptionsSelectedMessage(ArchiveModOptions.ToList()));
            }
        }
    }
}
