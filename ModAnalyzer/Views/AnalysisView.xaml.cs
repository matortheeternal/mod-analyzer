using System.Windows.Controls;

namespace ModAnalyzer.Views {
    /// <summary>
    /// Interaction logic for ResultsView.xaml
    /// </summary>
    public partial class AnalysisView : UserControl {
        private bool AutoScroll = true;

        public AnalysisView() {
            InitializeComponent();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0) {   
                // Content unchanged : user scroll event
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight) {   
                    // Scroll bar is in bottom -> Set autoscroll mode
                    AutoScroll = true;
                }
                else {   
                    // Scroll bar isn't in bottom -> Unset autoscroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : autoscroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0) {
                // Content changed and autoscroll mode set -> autoscroll
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
            }
        }
    }
}
