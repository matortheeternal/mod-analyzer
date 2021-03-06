using GalaSoft.MvvmLight;
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
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        public MainViewModel MainViewModel { get { return SimpleIoc.Default.GetInstance<MainViewModel>(); } }
        public AnalysisViewModel AnalysisViewModel { get { return SimpleIoc.Default.GetInstance<AnalysisViewModel>(); } }
        public HomeViewModel HomeViewModel { get { return SimpleIoc.Default.GetInstance<HomeViewModel>(); } }
        public ClassifyArchivesViewModel ClassifyArchivesViewModel { get { return SimpleIoc.Default.GetInstance<ClassifyArchivesViewModel>(); } }
        public ExtractArchivesViewModel ExtractArchivesViewModel { get { return SimpleIoc.Default.GetInstance<ExtractArchivesViewModel>(); } }
        public PluginMastersViewModel PluginMastersViewModel { get { return SimpleIoc.Default.GetInstance<PluginMastersViewModel>(); } }
        public SettingsViewModel SettingsViewModel { get { return SimpleIoc.Default.GetInstance<SettingsViewModel>(); } }

        public ViewModelBase ViewModelByName(string viewModelName) {
            return (ViewModelBase)GetType().GetProperty(viewModelName).GetValue(this, null);
        }

        public static ViewModelLocator Instance() {
            return (ViewModelLocator)App.Current.Resources["ViewModelLocator"];
        }
    }
}