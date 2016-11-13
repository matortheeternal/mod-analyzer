using System.Collections.Generic;

namespace ModAnalyzer.Messages
{
    public class FilesSelectedMessage
    {
        public FilesSelectedMessage(List<string> filePaths)
        {
            FilePaths = filePaths;
        }

        public List<string> FilePaths { get; }
    }
}
