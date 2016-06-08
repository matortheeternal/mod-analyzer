using GalaSoft.MvvmLight.Messaging;

namespace ModAnalyzer.Messages
{
    public class FileSelectedMessage : MessageBase
    {
        public string FilePath { get; set; }

        public FileSelectedMessage(string filePath)
        {
            FilePath = filePath;
        }
    }
}
