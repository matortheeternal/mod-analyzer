using ModAnalyzer.Domain.Services;
using System;
using System.Windows;

namespace ModAnalyzer {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            LogService.StartLogging();
            Closed += new EventHandler(MainWindow_Closed);
        }

        void MainWindow_Closed(object sender, EventArgs e) {
            LogService.StopLogging();
        }
    }
}
