using GalaSoft.MvvmLight;
using ModAnalyzer.Messages;
using ModAssetMapper;

namespace ModAnalyzer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ViewModelLocator _viewModelLocator;

        private string _progressMessage;

        public string ProgressMessage
        {
            get { return _progressMessage; }
            set { Set(nameof(ProgressMessage), ref _progressMessage, value); }
        }

        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set { Set(nameof(CurrentViewModel), ref _currentViewModel, value); }
        }
        
        public MainViewModel()
        {
            _viewModelLocator = (ViewModelLocator)App.Current.Resources["ViewModelLocator"];

            CurrentViewModel = _viewModelLocator.AnalysisViewModel;
            CurrentViewModel = _viewModelLocator.HomeViewModel;

            MessengerInstance.Register<FileSelectedMessage>(this, message => CurrentViewModel = _viewModelLocator.AnalysisViewModel);
            MessengerInstance.Register<ProgressMessage>(this, message => ProgressMessage = message.Message);
        }
    }
}