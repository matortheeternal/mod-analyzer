using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using ModAssetMapper;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels {
    public class AnalysisViewModel : ViewModelBase {
        private ModAnalyzerService _modAnalyzerService;

        public ICommand ResetCommand { get; set; }
        public ICommand ViewOutputCommand { get; set; }

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
            _modAnalyzerService = new ModAnalyzerService();
            _modAnalyzerService.MessageReported += _modAnalyzerService_MessageReported;

            ResetCommand = new RelayCommand(() => MessengerInstance.Send(new NavigationMessage(Page.Home)));
            ViewOutputCommand = new RelayCommand(() => Process.Start("output"));

            MessengerInstance.Register<ModOptionsSelectedMessage>(this, OnModOptionsSelected);
        }

        private void _modAnalyzerService_MessageReported(object sender, MessageReportedEventArgs e) {
            App.Current.Dispatcher.BeginInvoke((Action)(() => Log += e.Message + Environment.NewLine));

            if (e.IsStatusMessage)
                App.Current.Dispatcher.BeginInvoke((Action)(() => ProgressMessage = e.Message.Trim()));
        }

        private void OnModOptionsSelected(ModOptionsSelectedMessage message)
        {
            Log = string.Empty;

            _modAnalyzerService.AnalyzeMod(message.ModOptions);
        }        
    }
}
