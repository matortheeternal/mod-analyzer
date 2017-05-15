using ModAnalyzer.Domain.Services;
using System.Windows;

namespace ModAnalyzer {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            SettingsService.LoadSettings();
        } 
    }
}
