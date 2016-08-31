namespace ModAnalyzer.Domain {
    public class MessageReportedEventArgs {
        public string Message { get; set; }
        public bool IsStatusMessage { get; set; }

        public MessageReportedEventArgs(MessageType messageType, string message) {
            MessageType = messageType;
            Message = message;
            IsStatusMessage = isStatusMessage;
        }
    }
}