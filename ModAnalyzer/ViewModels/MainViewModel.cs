using System.Windows;
using GalaSoft.MvvmLight;
using ModAnalyzer.Messages;

namespace ModAnalyzer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ViewModelLocator _viewModelLocator;

        private ViewModelBase _currentViewModel;

        public MainViewModel()
        {
            _viewModelLocator = (ViewModelLocator) Application.Current.Resources["ViewModelLocator"];

            CurrentViewModel = _viewModelLocator.HomeViewModel;

            MessengerInstance.Register<NavigationMessage>(this, true, OnNavigationMessageReceived);
        }

        public ViewModelBase CurrentViewModel { get { return _currentViewModel; } set { Set(ref _currentViewModel, value); } }

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
