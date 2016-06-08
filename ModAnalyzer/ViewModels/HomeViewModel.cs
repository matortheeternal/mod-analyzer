using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Messages;
using System.Windows.Forms;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public ICommand BrowseCommand { get; set; }

        public HomeViewModel()
        {
            BrowseCommand = new RelayCommand(Browse);
        }

        private void Browse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Title = "Select a mod archive", Filter = "Archive Files (*.zip, *.7z, *.rar)|*.zip;*.7z;*.rar" };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
                MessengerInstance.Send(new FileSelectedMessage(openFileDialog.FileName));
        }
    }
}
