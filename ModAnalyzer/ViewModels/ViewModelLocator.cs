using GalaSoft.MvvmLight.Ioc;

namespace ModAnalyzer.ViewModels {
    public class ViewModelLocator {
        public ViewModelLocator() {
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<AnalysisViewModel>();
            SimpleIoc.Default.Register<HomeViewModel>();
            SimpleIoc.Default.Register<ClassifyArchivesViewModel>();
            SimpleIoc.Default.Register<ExtractArchivesViewModel>();
            SimpleIoc.Default.Register<PluginMastersViewModel>();
        }

        public MainViewModel MainViewModel { get { return SimpleIoc.Default.GetInstance<MainViewModel>(); } }
        public AnalysisViewModel AnalysisViewModel { get { return SimpleIoc.Default.GetInstance<AnalysisViewModel>(); } }
        public HomeViewModel HomeViewModel { get { return SimpleIoc.Default.GetInstance<HomeViewModel>(); } }
        public ClassifyArchivesViewModel ClassifyArchivesViewModel { get { return SimpleIoc.Default.GetInstance<ClassifyArchivesViewModel>(); } }
        public ExtractArchivesViewModel ExtractArchivesViewModel { get { return SimpleIoc.Default.GetInstance<ExtractArchivesViewModel>(); } }
        public PluginMastersViewModel PluginMastersViewModel { get { return SimpleIoc.Default.GetInstance<PluginMastersViewModel>(); } }
    }
}