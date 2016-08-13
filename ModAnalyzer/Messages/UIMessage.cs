namespace ModAnalyzer.Messages
{
    public enum MessageType
    {
        ProgressMessage,
        LogMessage
    }

    public class UIMessage
    {
        public MessageType MessageType { get; set; }
        public string Message { get; set; }

        public UIMessage(MessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message;
        }
    }
}
