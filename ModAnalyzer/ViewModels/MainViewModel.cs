using GalaSoft.MvvmLight;
using ModAnalyzer.Messages;

namespace ModAnalyzer.ViewModels {
    public class MainViewModel : ViewModelBase {
        private readonly ViewModelLocator _viewModelLocator;

        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel {
            get { return _currentViewModel; }
            set { Set(nameof(CurrentViewModel), ref _currentViewModel, value); }
        }

        public MainViewModel() {
            _viewModelLocator = ViewModelLocator.Instance();
            CurrentViewModel = _viewModelLocator.HomeViewModel;
            MessengerInstance.Register<NavigationMessage>(this, true, OnNavigationMessageReceived);
        }

        private void OnNavigationMessageReceived(NavigationMessage message) {
            //LogService.GroupMessage("views", "Navigated to " + message.ViewModelName);
            CurrentViewModel = _viewModelLocator.ViewModelByName(message.ViewModelName);
        }
    }
}