using System;
using System.Xml;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// Used to store information on a fomod file or folder node
    /// </summary>
    public class FomodFileNode {
        public bool IsFolder { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public int Priority { get; set; }

        public FomodFileNode(XmlNode node)
        {
            IsFolder = node.Name.Equals("folder");
            Source = node.Attributes["source"].Value;
            Destination = node.Attributes["destination"].Value;
            if (node.Attributes["priority"] != null)
            {
                Priority = Int32.Parse(node.Attributes["priority"].Value);
            }
        }

        public bool MatchesPath(string path) 
        {
            return path.StartsWith(Source + (IsFolder ? "\\" : ""));
        }

        public string MappedPath(string path) 
        {
            return path.Replace(Source + (IsFolder ? "\\" : ""), Destination);
        }
    }
}