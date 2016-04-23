using GalaSoft.MvvmLight.Ioc;

namespace ModAssetMapper.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<WelcomeViewModel>();
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