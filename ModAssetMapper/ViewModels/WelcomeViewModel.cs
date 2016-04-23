using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAssetMapper.Messages;
using System.Windows.Forms;
using System.Windows.Input;

namespace ModAssetMapper.ViewModels
{
    public class WelcomeViewModel : ViewModelBase
    {
        public ICommand BrowseCommand { get; set; }

        public WelcomeViewModel()
        {
            BrowseCommand = new RelayCommand(Browse);
        }

        private void Browse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Please select a file...",
                Filter = "Archive Files (*.zip, *.gzip, *.tar, *.rar, *.7z)|*.zip;*.gzip;*.tar;*.rar;*.7z"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MessengerInstance.Send(new NavigationMessage(ViewNames.AssetListView, openFileDialog.FileName));
            }
        }
    }
}
