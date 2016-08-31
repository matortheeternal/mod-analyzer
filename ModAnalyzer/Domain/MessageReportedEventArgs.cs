namespace ModAnalyzer.Domain {
    public class MessageReportedEventArgs {
        public string Message { get; set; }
        public bool IsStatusMessage { get; set; }

        public MessageReportedEventArgs(string message, bool isStatusMessage) {
            Message = message;
            IsStatusMessage = isStatusMessage;
        }
    }
}