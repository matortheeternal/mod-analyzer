using ModAnalyzer.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModAnalyzer.Views {
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl {
        public HomeView() {
            InitializeComponent();
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }

        private void UserControl_Drop(object sender, DragEventArgs e) {
            string[] fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop);
            ((HomeViewModel) DataContext).AnalyzeArchives(fileNames);
        }

        private void Settings_MouseDown(object sender, MouseButtonEventArgs e) {
            ((HomeViewModel) DataContext).GoToSettings();
        }
    }
}
