using GalaSoft.MvvmLight;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using System.Collections.ObjectModel;
using System.IO;

namespace ModAnalyzer.ViewModels {
    public class ClassifyArchivesViewModel : ViewModelBase {
        public ObservableCollection<ModOption> ModOptions { get; set; }

        public ClassifyArchivesViewModel() {
            ModOptions = new ObservableCollection<ModOption>();

            MessengerInstance.Register<FilesSelectedMessage>(this, OnFilesSelected);
        }

        private void OnFilesSelected(FilesSelectedMessage message) {
            ModOptions.Clear();

            foreach (string file in message.FilePaths)
                ModOptions.Add(new ModOption(Path.GetFileName(file), true, false));
        }
    }
}
