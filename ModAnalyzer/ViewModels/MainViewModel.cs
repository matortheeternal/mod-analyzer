using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
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
        }
        
        private void Browse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Title = "Select a mod archive", Filter = "Archive Files (*.zip, *.7z, *.rar)|*.zip;*.7z;*.rar" };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // toggle control visibility
                //toggleControlVisibility();

                LogMessages.Clear();
                
                ProgressMessage = "Loading " + openFileDialog.FileName + "...";

                // reset listing to empty
                //textBlock.Text = "";
                
                GetEntryMap(openFileDialog.FileName);
                
                string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                String filename = Path.Combine(rootPath, Path.GetFileNameWithoutExtension(openFileDialog.FileName));

                ProgressMessage = "Saving JSON to " + filename + ".json...";
                File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(_modAnalysis));
                ProgressMessage = "All done.  JSON file saved to "+ filename + ".json";
            }
        }

        
    }
}