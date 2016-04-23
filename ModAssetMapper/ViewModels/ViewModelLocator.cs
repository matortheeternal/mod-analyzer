using BA2Lib;
using GalaSoft.MvvmLight.Ioc;
using libbsa;
using ModAssetMapper.Services;

namespace ModAssetMapper.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<WelcomeViewModel>();
            SimpleIoc.Default.Register<BA2NET>();
            SimpleIoc.Default.Register<BSANET>();
            SimpleIoc.Default.Register<AssetArchiveService>();
            SimpleIoc.Default.Register<AssetListViewModel>();
        }

        public MainViewModel MainViewModel { get { return SimpleIoc.Default.GetInstance<MainViewModel>(); } }
        public WelcomeViewModel WelcomeViewModel { get { return SimpleIoc.Default.GetInstance<WelcomeViewModel>(); } }
        public AssetListViewModel AssetListViewModel { get { return SimpleIoc.Default.GetInstance<AssetListViewModel>(); } }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}