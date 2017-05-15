using GalaSoft.MvvmLight;
using ModAnalyzer.Messages;
using ModAnalyzer.Analysis.Events;
using System;
using System.Collections.Generic;
using ModAnalyzer.Analysis.Services;
using ModAnalyzer.Analysis.Models;
using ModAnalyzer.Domain.Services;

namespace ModAnalyzer.ViewModels {
    public class ExtractArchivesViewModel : ViewModelBase {
        private ArchiveService _archiveService;
        private string _progressMessage;
        public List<ModOption> ModOptions { get; set; }

        public string ProgressMessage {
            get { return _progressMessage; }
            set { Set(nameof(ProgressMessage), ref _progressMessage, value); }
        }

        public ExtractArchivesViewModel() {
            ProgressMessage = "Extracting archives...";
            _archiveService = new ArchiveService();
            _archiveService.MessageReported += _archiveService_MessageReported;
            _archiveService.ArchivesExtracted += _archiveService_ArchivesExtracted;
        }

        private void _archiveService_MessageReported(object sender, MessageReportedEventArgs e) {
            LogService.GroupMessage("analysis", e.Message);
            if (e.IsStatusMessage) {
                App.Current.Dispatcher.BeginInvoke((Action)(() => ProgressMessage = e.Message.Trim()));
            }
        }

        private void _archiveService_ArchivesExtracted(object sender, ArchivesExtractedEventArgs e) {
            if (e.HasMissingMasters) {
                MessengerInstance.Send(new NavigationMessage("PluginMasters"));
                ViewModelLocator.Instance().PluginMastersViewModel.InitMissingMasters(e.MissingMasters, ModOptions);
            }
            else {
                MessengerInstance.Send(new NavigationMessage("Analysis"));
                ViewModelLocator.Instance().AnalysisViewModel.StartAnalysis(ModOptions);
            }
        }

        public void OnArchivesClassified(List<ModOption> ModOptions) {
            this.ModOptions = ModOptions;
            _archiveService.ExtractArchives(ModOptions);
        }
    }
}