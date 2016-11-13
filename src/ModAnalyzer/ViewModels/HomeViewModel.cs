using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModAnalyzer.Domain;
using ModAnalyzer.Messages;
using ModAnalyzer.Properties;
using ModAnalyzer.Utils;
using WPFDragEventArgs = System.Windows.DragEventArgs;

namespace ModAnalyzer.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private static readonly string[] ArchiveExts =
        {
            ".zip", ".7z", ".rar"
        };

        private ICommand _browseCommand;
        private ICommand _dropCommand;

        private bool _isDialogOpen;

        private bool _isUpdateAvailable;
        private ICommand _updateCommand;

        public HomeViewModel()
        {
            CheckForUpdate();
        }

        public ICommand BrowseCommand { get { return _browseCommand ?? (_browseCommand = new RelayCommand(Browse)); } }

        public ICommand UpdateCommand { get { return _updateCommand ?? (_updateCommand = new RelayCommand(OpenDownloadPage)); } }

        public bool IsUpdateAvailable { get { return _isUpdateAvailable; } set { Set(ref _isUpdateAvailable, value); } }

        public bool IsDialogOpen { get { return _isDialogOpen; } set { Set(ref _isDialogOpen, value); } }
        public ICommand DropCommand { get { return _dropCommand ?? (_dropCommand = new RelayCommand<WPFDragEventArgs>(OnDrop)); } }

        private void OnDrop(WPFDragEventArgs e)
        {
            if (e == null)
                return;
            var fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            AnalyzeArchives(fileNames);
        }

        private static void OpenDownloadPage()
        {
            try
            {
                Process.Start("https://github.com/matortheeternal/mod-analyzer/releases");
            }
            catch (Exception exception)
            {
                MessageBox.Show(Resources.Error_Failed_to_open_download_page + Environment.NewLine + exception, Resources.Title_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Browse()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = Resources.Title_Browse, Filter = Resources.Filter_Archives, Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
                AnalyzeArchives(openFileDialog.FileNames);
        }

        public void AnalyzeArchives(IEnumerable<string> fileNames)
        {
            var validArchives = fileNames.Where(fileName => ArchiveExts.Contains(Path.GetExtension(fileName))).ToList();
            if (validArchives.Count == 0)
                return;

            if (validArchives.Count == 1)
            {
                var file = validArchives[0];
                var defaultOption = new ModOption(Path.GetFileName(file), true, false)
                {
                    SourceFilePath = file
                };
                var archiveModOptions = new List<ModOption>
                {
                    defaultOption
                };
                MessengerInstance.Send(new ArchiveModOptionsSelectedMessage(archiveModOptions));
            } else
            {
                MessengerInstance.Send(new FilesSelectedMessage(validArchives));
                IsDialogOpen = true;
            }
        }

        private async void CheckForUpdate()
        {
            try
            {
                IsUpdateAvailable = await UpdateUtil.IsUpdateAvailable();
            }
            catch (Exception exception)
            {
                IsUpdateAvailable = false;
                MessageBox.Show(Resources.Error_Failed_to_check_for_updates + Environment.NewLine + exception);
            }
        }
    }
}
