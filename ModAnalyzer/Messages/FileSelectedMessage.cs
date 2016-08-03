namespace ModAnalyzer.Messages
{
    public class FileSelectedMessage : NavigationMessage
    {
        public string FilePath { get; set; }

        public FileSelectedMessage(string filePath)
            : base(Page.Analysis)
        {
            FilePath = filePath;
        }
    }
}
