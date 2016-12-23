using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModAnalyzer.Messages {
    public class FilesSelectedMessage : NavigationMessage {
        public List<string> FilePaths { get; set; }
        public bool ReplaceSelection { get; set; }
        private static readonly string[] archiveExts = { ".zip", ".7z", ".rar" };

        public FilesSelectedMessage(List<string> filePaths, bool replace) : base(Page.ClassifyArchives) {
            FilePaths = filePaths;
            ReplaceSelection = replace;
        }

        public static void SelectArchives(string[] fileNames, IMessenger MessengerInstance, bool replace) {
            List<string> validArchives = fileNames.Where(
                fileName => archiveExts.Contains(Path.GetExtension(fileName))
            ).ToList();
            if (validArchives.Count == 0) return;

            MessengerInstance.Send(new FilesSelectedMessage(validArchives, replace));
        }
    }
}