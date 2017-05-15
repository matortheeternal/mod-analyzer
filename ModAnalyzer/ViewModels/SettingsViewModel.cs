using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ModAnalyzer.Domain.Services;
using ModAnalyzer.Messages;
using ModAnalyzer.Utils;

namespace ModAnalyzer.ViewModels {
    public class SettingsViewModel : ViewModelBase {
        public string SkyrimPath {
            get {
                return SettingsService.Settings.SkyrimPath;
            }
            set {
                SettingsService.Settings.SkyrimPath = value;
                RaisePropertyChanged("SkyrimPath");
            }
        }
        public string SkyrimSEPath {
            get {
                return SettingsService.Settings.SkyrimSEPath;
            }
            set {
                SettingsService.Settings.SkyrimSEPath = value;
                RaisePropertyChanged("SkyrimSEPath");
            }
        }
        public RelayCommand SaveCommand { get; private set; }

        public SettingsViewModel() {
            SaveCommand = new RelayCommand(Save);
        }

        public void BrowseSkyrimPath() {
            string gamePath = DialogUtils.BrowseFolder("Select your Skyrim data folder.");
            if (!string.IsNullOrEmpty(gamePath)) {
                SkyrimPath = gamePath;
            }
        }

        public void BrowseSkyrimSEPath() {
            string gamePath = DialogUtils.BrowseFolder("Select your Skyrim Special Edition data folder.");
            if (!string.IsNullOrEmpty(gamePath)) {
                SkyrimSEPath = gamePath;
            }
        }

        private void Save() {
            SettingsService.SaveSettings();
            MessengerInstance.Send(new NavigationMessage("Home"));
        }
    }
}