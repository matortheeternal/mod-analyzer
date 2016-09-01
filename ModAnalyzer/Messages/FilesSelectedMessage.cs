using System.Collections.Generic;

namespace ModAnalyzer.Messages {
    public class FilesSelectedMessage : NavigationMessage {
        public List<string> FilePaths { get; set; }

        public FilesSelectedMessage(List<string> filePaths)
            : base(Page.Analysis) {
            FilePaths = filePaths;
        }
    }
}