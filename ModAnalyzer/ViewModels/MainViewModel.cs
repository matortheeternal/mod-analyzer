using GalaSoft.MvvmLight;
using ModAnalyzer.Messages;
using ModAssetMapper;

namespace ModAnalyzer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ViewModelLocator _viewModelLocator;

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