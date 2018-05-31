using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Messages;
using ModAnalyzer.Analysis.Events;
using ModAnalyzer.Analysis.Services;
using ModAnalyzer.Analysis.Models;
using System;
using System.Diagnostics;
using System.Windows.Input;
using System.Collections.Generic;
using ModAnalyzer.Domain.Services;

namespace ModAnalyzer.ViewModels {
    public class AnalysisViewModel : ViewModelBase {
        private ModAnalyzerService _modAnalyzerService;

        public ICommand ResetCommand { get; set; }
        public ICommand ViewOutputCommand { get; set; }
        public bool CanReset { get; set; }

        private string _log;

        public string Log {
            get { return _log; }
            set { Set(nameof(Log), ref _log, value); }
        }

        private string _progressMessage;

        public string ProgressMessage {
            get { return _progressMessage; }
            set { Set(nameof(ProgressMessage), ref _progressMessage, value); }
        }

        public AnalysisViewModel() {
            CanReset = false;

            ResetCommand = new RelayCommand(Reset);
            ViewOutputCommand = new RelayCommand(() => Process.Start("output"));
        }

        private void Reset() {
            MessengerInstance.Send(new NavigationMessage("Home"));
        }

        private void _modAnalyzerService_MessageReported(object sender, MessageReportedEventArgs e) {
            App.Current.Dispatcher.BeginInvoke((Action)(() => {
                Log += e.Message + Environment.NewLine;
                if (!string.IsNullOrWhiteSpace(e.Message))
                    LogService.GroupMessage("analysis", e.Message);
                if (e.IsStatusMessage)
                    ProgressMessage = e.Message.Trim();
            }));
        }

        private void _modAnalyzerService_AnalysisComplete(object sender, EventArgs e) {
            App.Current.Dispatcher.BeginInvoke((Action)(() => {
                LogService.GroupMessage("analysis", "Analysis complete.");
                CanReset = true;
                RaisePropertyChanged("CanReset");
            }));
        }

        private void StartModAnalyzerService() {
            if (_modAnalyzerService != null) return;
            _modAnalyzerService = new ModAnalyzerService();
            _modAnalyzerService.MessageReported += _modAnalyzerService_MessageReported;
            _modAnalyzerService.AnalysisCompleted += _modAnalyzerService_AnalysisComplete;
        }

        public void StartAnalysis(List<ModOption> ModOptions) {
            Log = string.Empty;
            StartModAnalyzerService();
            _modAnalyzerService.AnalyzeMod(ModOptions);
        }
    }
}