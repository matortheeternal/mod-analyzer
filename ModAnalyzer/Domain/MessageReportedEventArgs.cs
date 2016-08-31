namespace ModAnalyzer.Domain {
    public enum MessageType {
        ProgressMessage,
        LogMessage
    }

    public class MessageReportedEventArgs {
        public MessageType MessageType { get; set; }
        public string Message { get; set; }

        public MessageReportedEventArgs(MessageType messageType, string message) {
            MessageType = messageType;
            Message = message;
        }
    }
}