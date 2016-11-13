using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;

namespace ModAnalyzer.ViewModels
{
    public class ClassifyArchivesViewModel : ViewModelBase
    {
        private ICommand _analyzeCommand;

        public ClassifyArchivesViewModel()
        {
            MessengerInstance.Register<FilesSelectedMessage>(this, OnFilesSelected);
        }

        public ObservableCollection<ModOption> ArchiveModOptions { get; } = new ObservableCollection<ModOption>();
        public ICommand AnalyzeCommand { get { return _analyzeCommand ?? (_analyzeCommand = new RelayCommand(AnalyzeMod)); } }

        private void OnFilesSelected(FilesSelectedMessage message)
        {
            ArchiveModOptions.Clear();
            foreach (var file in message.FilePaths)
                ArchiveModOptions.Add(new ModOption(Path.GetFileName(file), false, false)
                {
                    SourceFilePath = file
                });
        }

        private void AnalyzeMod()
        {
            MessengerInstance.Send(new ArchiveModOptionsSelectedMessage(ArchiveModOptions.ToList()));
        }
    }
}
