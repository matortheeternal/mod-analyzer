using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class ClassifyArchivesViewModel : ViewModelBase {
        public ObservableCollection<ModOption> ModOptions { get; set; }
        public ICommand AnalyzeCommand { get; set; }

        public ClassifyArchivesViewModel() {
            ModOptions = new ObservableCollection<ModOption>();
            AnalyzeCommand = new RelayCommand(() => MessengerInstance.Send(new ModOptionsSelectedMessage(ModOptions)));

            MessengerInstance.Register<FilesSelectedMessage>(this, OnFilesSelected);
        }

        private void OnFilesSelected(FilesSelectedMessage message) {
            ModOptions.Clear();

            foreach (string file in message.FilePaths)
                ModOptions.Add(new ModOption(file, true, false));
        }
    }
}
