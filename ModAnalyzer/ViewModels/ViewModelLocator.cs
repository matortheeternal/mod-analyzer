using GalaSoft.MvvmLight.Ioc;

namespace ModAnalyzer.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<AnalysisViewModel>();
            SimpleIoc.Default.Register<HomeViewModel>();
        }

        public MainViewModel MainViewModel { get { return SimpleIoc.Default.GetInstance<MainViewModel>(); } }
        public AnalysisViewModel AnalysisViewModel { get { return SimpleIoc.Default.GetInstance<AnalysisViewModel>(); } }
        public HomeViewModel HomeViewModel { get { return SimpleIoc.Default.GetInstance<HomeViewModel>(); } }
    }
}