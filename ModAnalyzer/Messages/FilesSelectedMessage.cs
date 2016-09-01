using System.Collections.Generic;

namespace ModAnalyzer.Messages {
    public class FilesSelectedMessage {
        public List<string> FilePaths { get; set; }

        public FilesSelectedMessage(List<string> filePaths) {
            FilePaths = filePaths;
        }
    }
}