using System;
using System.Windows.Controls;

namespace ModAnalyzer.Views
{
    /// <summary>
    ///     Interaction logic for ResultsView.xaml
    /// </summary>
    public partial class AnalysisView
    {
        private bool _autoScroll = true;

        public AnalysisView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (Math.Abs(e.ExtentHeightChange) < 1E-5)
                if (Math.Abs(ScrollViewer.VerticalOffset - ScrollViewer.ScrollableHeight) < 1E-5)
                    _autoScroll = true;
                else
                    _autoScroll = false;

            // Content scroll event : autoscroll eventually
            if (_autoScroll && (Math.Abs(e.ExtentHeightChange) > 0))
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ExtentHeight);
        }
    }
}
