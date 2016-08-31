namespace ModAnalyzer.Domain {
    public static class MessageReportedEventArgsFactory {
        public static MessageReportedEventArgs CreateLogMessageEventArgs(string message) {
            return new MessageReportedEventArgs(MessageType.LogMessage, message);
        }

        public static MessageReportedEventArgs CreateProgressMessageEventArgs(string message) {
            return new MessageReportedEventArgs(MessageType.ProgressMessage, message);
        }
    }
}