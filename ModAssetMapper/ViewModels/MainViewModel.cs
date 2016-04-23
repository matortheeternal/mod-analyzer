using GalaSoft.MvvmLight;
using ModAssetMapper.Messages;

namespace ModAssetMapper.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ViewModelLocator _viewModelLocator;
        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                _currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }

        public MainViewModel()
        {
            _viewModelLocator = new ViewModelLocator();

            CurrentViewModel = _viewModelLocator.WelcomeViewModel;

            MessengerInstance.Register<NavigationMessage>(this, Navigate);
        }

        private void Navigate(NavigationMessage navigationMessage)
        {
            switch (navigationMessage.ViewName)
            {
                case ViewNames.AssetListView:
                    _viewModelLocator.AssetListViewModel.ArchiveFilePath = navigationMessage.Model.ToString();
                    CurrentViewModel = _viewModelLocator.AssetListViewModel;
                    break;
                default:
                    break;
            }
        }
    }
}