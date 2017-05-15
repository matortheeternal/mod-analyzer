using GalaSoft.MvvmLight;
using ModAnalyzer.Domain.Services;
using ModAnalyzer.Messages;
using System;

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
            SettingsService.LoadSettings();
            CurrentViewModel = _viewModelLocator.HomeViewModel;
            if (SettingsService.NewSettings) {
                HandleSettings();
            }
            MessengerInstance.Register<NavigationMessage>(this, true, OnNavigationMessageReceived);
        }

        public void HandleSettings() {
            try {
                //LogService.GroupMessage("settings", "Navigated to SettingsView for initial set up.");
                CurrentViewModel = _viewModelLocator.SettingsViewModel;
                _viewModelLocator.SettingsViewModel.CreateNewSettings();
            }
            catch (Exception x) {
                //LogService.GroupMessage("settings", "Exception navigating to settings view, " + x.Message);
            }
        }

        private void OnNavigationMessageReceived(NavigationMessage message) {
            //LogService.GroupMessage("views", "Navigated to " + message.ViewModelName);
            CurrentViewModel = _viewModelLocator.ViewModelByName(message.ViewModelName);
        }
    }
}