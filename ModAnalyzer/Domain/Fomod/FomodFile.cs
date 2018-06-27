using ModAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace ModAnalyzer.Domain.Fomod {
    /// <summary>
    /// Used to store information on a fomod file or folder node
    /// </summary>
    public class FomodFile {
        public bool IsFolder { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public int Priority { get; set; }
        private string CompSource { get; set; }
        private string Replace { get; set; }
        private Regex Expr { get; set; }

        public FomodFile(XmlNode node) {
            IsFolder = node.Name.Equals("folder");
            Source = node.Attributes["source"].Value.Replace('\t', ' ').Replace('/', '\\');
            if (IsFolder) Source = PathExtensions.AppendDelimiter(Source);
            if (node.Attributes["destination"] != null) {
                Destination = node.Attributes["destination"].Value.Replace('\t', ' ').Replace('/', '\\');
                if (IsFolder && !string.IsNullOrEmpty(Destination))
                    Destination = PathExtensions.AppendDelimiter(Destination);
            } else {
                Destination = "";
            }
            if (node.Attributes["priority"] != null) {
                Priority = Int32.Parse(node.Attributes["priority"].Value);
            }

            Expr = new Regex("^" + Regex.Escape(Source), RegexOptions.IgnoreCase);
        }

        public static List<FomodFile> FromNodes(XmlNodeList nodes) {
            List<FomodFile> files = new List<FomodFile>();
            foreach (XmlNode node in nodes) {
                if (node.Name != "folder" && node.Name != "file") continue;
                files.Add(new FomodFile(node));
            }
            return files;
        }

        public bool MatchesPath(string path) {
            return path.StartsWith(Source, StringComparison.CurrentCultureIgnoreCase);
        }

        public string MappedPath(string path) {
            if (!IsFolder && string.IsNullOrEmpty(Destination)) return path.Replace("/", @"\");
            return Expr.Replace(path, Destination).Replace("/", @"\");
        }
    }
}