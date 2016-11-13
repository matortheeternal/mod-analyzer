using System.Windows;
using ModAnalyzer.ViewModels;

namespace ModAnalyzer.Views
{
    public partial class HomeView
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            var fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop);
            ((HomeViewModel) DataContext).AnalyzeArchives(fileNames);
        }
    }
}
