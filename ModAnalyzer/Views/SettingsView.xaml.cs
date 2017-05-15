using ModAnalyzer.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModAnalyzer.Views {
    public partial class SettingsView : UserControl {
        public SettingsViewModel ViewModel {
            get {
                return (SettingsViewModel) DataContext;
            }
        }

        public SettingsView() {
            InitializeComponent();
        }

        private void BrowseSkyrimSEPath_MouseDown(object sender, MouseButtonEventArgs e) {
            ViewModel.BrowseSkyrimSEPath();
        }

        private void BrowseSkyrimPath_MouseDown(object sender, MouseButtonEventArgs e) {
            ViewModel.BrowseSkyrimPath();
        }
    }
}