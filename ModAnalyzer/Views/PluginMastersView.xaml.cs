using ModAnalyzer.ViewModels;
using System.Windows.Controls;

namespace ModAnalyzer.Views {
    public partial class PluginMastersView : UserControl {
        public PluginMastersView() {
            InitializeComponent();
        }

        private void PackIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            ((PluginMastersViewModel) DataContext).Back();
        }
    }
}