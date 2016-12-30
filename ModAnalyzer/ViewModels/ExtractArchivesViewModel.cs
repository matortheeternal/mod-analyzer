using GalaSoft.MvvmLight;
using ModAnalyzer.Messages;
using ModAnalyzer.Domain;
using ModAssetMapper;
using System;
using System.Collections.Generic;

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

            MessengerInstance.Register<ArchivesClassifiedMessage>(this, OnArchivesClassified);
        }

        private void _archiveService_MessageReported(object sender, MessageReportedEventArgs e) {
            if (e.IsStatusMessage) {
                App.Current.Dispatcher.BeginInvoke((Action)(() => ProgressMessage = e.Message.Trim()));
            }
        }

        private void _archiveService_ArchivesExtracted(object sender, ArchivesExtractedEventArgs e) {
            if (e.HasMissingMasters) {
                MessengerInstance.Send(new MissingMastersMessage(e.MissingMasters, ModOptions));
            } else {
                MessengerInstance.Send(new AnalyzeArchivesMessage(ModOptions));
            }
        }

        private void OnArchivesClassified(ArchivesClassifiedMessage message) {
            ModOptions = message.ModOptions;
            _archiveService.ExtractArchives(ModOptions);
        }
    }
}