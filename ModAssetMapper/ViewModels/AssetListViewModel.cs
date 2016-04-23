using GalaSoft.MvvmLight;
using ModAssetMapper.Services;
using System.Collections.ObjectModel;

namespace ModAssetMapper.ViewModels
{
    public class AssetListViewModel : ViewModelBase
    {
        private readonly AssetArchiveService _assetArchiveService;

        private string _archiveFilePath;

        public string ArchiveFilePath
        {
            get { return _archiveFilePath; }
            set
            {
                _archiveFilePath = value;

                LoadAssets();
            }
        }

        public ObservableCollection<string> Assets { get; set; }

        public AssetListViewModel(AssetArchiveService assetArchiveService)
        {
            _assetArchiveService = assetArchiveService;

            Assets = new ObservableCollection<string>();
        }

        private void LoadAssets()
        {
            Assets.Clear();

            foreach (string entry in _assetArchiveService.GetEntryMap(ArchiveFilePath))
                Assets.Add(entry);
        }
    }
}
