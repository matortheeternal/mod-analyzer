using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;

namespace ModAnalyzer.ViewModels
{
    public class AnalysisViewModel : ViewModelBase
    {
        private readonly ModAnalyzerService _modAnalyzerService;
        private string _log;
        private string _progressMessage;
        private ICommand _resetCommand;
        private ICommand _viewOutputCommand;

        public AnalysisViewModel()
        {
            _modAnalyzerService = new ModAnalyzerService();
            _modAnalyzerService.MessageReported += _modAnalyzerService_MessageReported;
            MessengerInstance.Register<ArchiveModOptionsSelectedMessage>(this, OnArchiveModOptionsSelected);
        }

        public ICommand ResetCommand
        {
            get
            {
                return _resetCommand ?? (_resetCommand = new RelayCommand(() =>
                       {
                           Log = string.Empty;
                           MessengerInstance.Send(new NavigationMessage(Page.Home));
                       }));
            }
        }

        public ICommand ViewOutputCommand { get { return _viewOutputCommand ?? (_viewOutputCommand = new RelayCommand(() => Process.Start("output"))); } }

        public string Log { get { return _log; } set { Set(ref _log, value); } }

        public string ProgressMessage { get { return _progressMessage; } set { Set(ref _progressMessage, value); } }

        private void _modAnalyzerService_MessageReported(object sender, MessageReportedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Log += e.Message + Environment.NewLine;
                if (e.IsStatusMessage)
                    ProgressMessage = e.Message.Trim();
            }));
        }

        private void OnArchiveModOptionsSelected(ArchiveModOptionsSelectedMessage message)
        {
            Log = string.Empty;
            _modAnalyzerService.AnalyzeMod(message.ModOptions);
        }
    }
}
