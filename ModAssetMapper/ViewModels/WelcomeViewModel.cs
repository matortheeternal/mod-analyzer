using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAssetMapper.Messages;
using SharpCompress.Archive;
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
                Filter = "Zip File (*.zip)|*.zip|GZip (*.gzip)|*.gzip|Tar (*.tar)|*.tar|Rar (*.rar)|*.rar|7Zip (*.7z)|*.7z"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MessengerInstance.Send(new NavigationMessage(ViewNames.AssetListView, openFileDialog.FileName));
            }
        }
    }
}
