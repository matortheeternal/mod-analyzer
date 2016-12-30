using GalaSoft.MvvmLight.Messaging;

namespace ModAnalyzer.Messages {
    public enum Page {
        Analysis,
        Home,
        ClassifyArchives,
        ExtractArchives,
        PluginMasters
    }

    public class NavigationMessage : MessageBase {
        public Page Page { get; set; }

        public NavigationMessage(Page page) {
            Page = page;
        }
    }
}
