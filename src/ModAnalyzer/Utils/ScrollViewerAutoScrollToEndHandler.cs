using System;
using System.Windows;
using System.Windows.Controls;

namespace ModAnalyzer.Utils
{
    public class ScrollViewerAutoScrollToEndHandler : DependencyObject, IDisposable
    {
        private static readonly double Tolerance = 1E-1;
        private readonly ScrollViewer _scrollViewer;
        private bool _doScroll;

        public ScrollViewerAutoScrollToEndHandler(ScrollViewer scrollViewer)
        {
            if (scrollViewer == null)
                throw new ArgumentNullException(nameof(scrollViewer));
            _scrollViewer = scrollViewer;
            _scrollViewer.ScrollToEnd();
            _scrollViewer.ScrollChanged += ScrollChanged;
        }

        public void Dispose()
        {
            _scrollViewer.ScrollChanged -= ScrollChanged;
        }

        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (Math.Abs(e.ExtentHeightChange) < Tolerance)
                _doScroll = Math.Abs(_scrollViewer.VerticalOffset - _scrollViewer.ScrollableHeight) < Tolerance;

            // Content scroll event : autoscroll eventually
            if (_doScroll && (Math.Abs(e.ExtentHeightChange) > Tolerance))
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.ExtentHeight);
        }
    }
}
