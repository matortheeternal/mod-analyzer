using GalaSoft.MvvmLight.Messaging;

namespace ModAnalyzer.Messages {
    public class NavigationMessage : MessageBase {
        public string ViewModelName { get; set; }

        public NavigationMessage(string BaseName) {
            ViewModelName = BaseName + "ViewModel";
        }
    }
}
