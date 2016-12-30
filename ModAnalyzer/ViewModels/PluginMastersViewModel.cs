using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class PluginMastersViewModel : ViewModelBase {
        public ObservableCollection<MissingMaster> MissingMasters { get; set; }
        public List<ModOption> ModOptions { get; set; }
        public ICommand ContinueCommand { get; set; }

        public PluginMastersViewModel() {
            MissingMasters = new ObservableCollection<MissingMaster>();
            ContinueCommand = new RelayCommand(Continue);
            MessengerInstance.Register<MissingMastersMessage>(this, OnMissingMasters);
        }

        private void OnMissingMasters(MissingMastersMessage message) {
            foreach (var item in message.MissingMasters) MissingMasters.Add(item);
            ModOptions = message.ModOptions;
        }

        private void Continue() {
            MessengerInstance.Send(new AnalyzeArchivesMessage(ModOptions));
        }

        public void Back() {
            MessengerInstance.Send(new NavigationMessage(Page.ClassifyArchives));
        }
    }
}