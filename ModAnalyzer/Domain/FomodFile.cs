using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// Used to store information on a fomod file or folder node
    /// </summary>
    public class FomodFile {
        public bool IsFolder { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public int Priority { get; set; }

        public FomodFile(XmlNode node) {
            IsFolder = node.Name.Equals("folder");
            Source = node.Attributes["source"].Value;
            Destination = node.Attributes["destination"].Value;
            if (node.Attributes["priority"] != null) {
                Priority = Int32.Parse(node.Attributes["priority"].Value);
            }
        }

        public static List<FomodFile> FromNodes(XmlNodeList nodes) {
            List<FomodFile> files = new List<FomodFile>();
            foreach (XmlNode node in nodes) {
                files.Add(new FomodFile(node));
            }
            return files;
        }

        public bool MatchesPath(string path) {
            return path.StartsWith(IsFolder ? Path.Combine(Source, "") : Source);
        }

        public string AppendPathDelimiter(string path) {
            if (path.EndsWith(@"\")) {
                return path;
            } else {
                return path + @"\";
            }
        }

        public string ReplaceFolderPath(string path) {
            if (Destination != "") {
                return path.Replace(AppendPathDelimiter(Source), AppendPathDelimiter(Destination));
            } else {
                return path.Replace(AppendPathDelimiter(Source), Destination);
            }
        }

        public string MappedPath(string path) {
            if (IsFolder) {
                return ReplaceFolderPath(path).Replace("/", @"\");
            } else {
                return path.Replace(Source, Destination).Replace("/", @"\");
            }
        }
    }
}