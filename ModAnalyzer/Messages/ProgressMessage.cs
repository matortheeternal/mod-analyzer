using GalaSoft.MvvmLight.Messaging;

namespace ModAnalyzer.Messages
{
    public class ProgressMessage : MessageBase
    {
        public string Message { get; set; }

        public ProgressMessage(string message)
        {
            Message = message;
        }
    }
}
