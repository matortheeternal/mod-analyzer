using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ModAnalyzer.Domain
{
    /// <summary>
    ///     Used to store information on a fomod file or folder node
    /// </summary>
    public class FomodFile
    {
        public FomodFile(XmlNode node)
        {
            IsFolder = node.Name.Equals("folder");
            if (node.Attributes != null)
            {
                Source = node.Attributes["source"].Value;
                Destination = node.Attributes["destination"].Value;
                if (node.Attributes["priority"] != null)
                    Priority = int.Parse(node.Attributes["priority"].Value);
            }
        }

        public bool IsFolder { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public int Priority { get; set; }

        public static List<FomodFile> FromNodes(XmlNodeList nodes)
        {
            var files = new List<FomodFile>();
            foreach (XmlNode node in nodes)
                files.Add(new FomodFile(node));
            return files;
        }

        public bool MatchesPath(string path)
        {
            return path.StartsWith(IsFolder ? Path.Combine(Source, "") : Source);
        }

        public string MappedPath(string path)
        {
            return path.Replace(Source + (IsFolder ? "\\" : ""), Destination);
        }
    }
}
