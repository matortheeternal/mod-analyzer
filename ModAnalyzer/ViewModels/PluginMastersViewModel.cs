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
            ContinueCommand = new RelayCommand(Continue);
        }

        public void InitMissingMasters(List<MissingMaster> masters, List<ModOption> options) {
            MissingMasters = new ObservableCollection<MissingMaster>();
            foreach (var item in masters) {
                LogService.GroupMessage("analysis", "Missing master: " + item.FileName);
                MissingMasters.Add(item);
            }
            ModOptions = options;
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