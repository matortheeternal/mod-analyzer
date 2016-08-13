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
            
            CurrentViewModel = _viewModelLocator.HomeViewModel;

            MessengerInstance.Register<NavigationMessage>(this, true, OnNavigationMessageReceived);
            MessengerInstance.Register<UIMessage>(this, message => ProgressMessage = message.Message);
        }

        private void OnNavigationMessageReceived(NavigationMessage message)
        {
            switch (message.Page)
            {
                case Page.Analysis:
                    CurrentViewModel = _viewModelLocator.AnalysisViewModel;
                    break;
                case Page.Home:
                    CurrentViewModel = _viewModelLocator.HomeViewModel;
                    break;
            }
        }
    }
}