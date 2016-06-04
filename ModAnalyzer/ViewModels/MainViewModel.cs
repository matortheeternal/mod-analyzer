using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Messages;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Input;

namespace ModAnalyzer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ICommand BrowseCommand { get; set; }
        
        private string _progressMessage;

        public string ProgressMessage
        {
            get { return _progressMessage; }
            set { Set(nameof(ProgressMessage), ref _progressMessage, value); }
        }

        public MainViewModel()
        {
            BrowseCommand = new RelayCommand(Browse);

            MessengerInstance.Register<string>(this, message => ProgressMessage = message);
        }
        
        private void Browse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Title = "Select a mod archive", Filter = "Archive Files (*.zip, *.7z, *.rar)|*.zip;*.7z;*.rar" };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ProgressMessage = "Loading " + openFileDialog.FileName + "...";

                MessengerInstance.Send(new FileSelectedMessage(openFileDialog.FileName));
            }
        }
    }
}