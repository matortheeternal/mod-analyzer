using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ModAnalyzer.Messages;
using ModAnalyzer.Analysis.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ModAnalyzer.Domain.Services;

namespace ModAnalyzer.ViewModels {
    public class PluginMastersViewModel : ViewModelBase {
        public ObservableCollection<MissingMaster> MissingMasters { get; set; }
        public List<ModOption> ModOptions { get; set; }
        public ICommand ContinueCommand { get; set; }

        public PluginMastersViewModel() {
            MissingMasters = new ObservableCollection<MissingMaster>();
            ContinueCommand = new RelayCommand(Continue);
        }

        public void InitMissingMasters(List<MissingMaster> MissingMasters, List<ModOption> ModOptions) {
            foreach (var item in MissingMasters) {
                LogService.GroupMessage("analysis", "Missing master: " + item.FileName);
                this.MissingMasters.Add(item);
            }
            ModOptions = this.ModOptions;
        }

        private void Continue() {
            MessengerInstance.Send(new NavigationMessage("Analysis"));
            ViewModelLocator.Instance().AnalysisViewModel.StartAnalysis(ModOptions);
        }

        public void Back() {
            MessengerInstance.Send(new NavigationMessage("ClassifyArchives"));
        }
    }
}