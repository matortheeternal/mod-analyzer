using GalaSoft.MvvmLight.Messaging;

namespace ModAssetMapper.Messages
{
    public enum ViewNames
    {
        WelcomeView,
        AssetListView
    }

    public class NavigationMessage : MessageBase
    {
        public ViewNames ViewName { get; set; }
        public object Model { get; set; }

        public NavigationMessage(ViewNames viewName, object model)
        {
            ViewName = viewName;
            Model = model;
        }
    }
}
