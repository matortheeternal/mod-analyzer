using GalaSoft.MvvmLight.Messaging;

namespace ModAnalyzer.Messages
{
    public enum Page
    {
        Analysis,
        Home
    }

    public class NavigationMessage : MessageBase
    {
        public NavigationMessage(Page page)
        {
            Page = page;
        }

        public Page Page { get; }
    }
}
