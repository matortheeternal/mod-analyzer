using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (node.Attributes == null)
                return;
            Source = node.Attributes["source"].Value;
            Destination = node.Attributes["destination"].Value;
            if (node.Attributes["priority"] != null)
                Priority = Convert.ToInt32(node.Attributes["priority"].Value);
        }

        public bool IsFolder { get; }
        public string Source { get; }
        public string Destination { get; }
        public int Priority { get; }

        public static List<FomodFile> FromNodes(XmlNodeList nodes)
        {
            return nodes.Cast<XmlNode>().Select(node => new FomodFile(node)).ToList();
        }

        public bool MatchesPath(string path)
        {
            return path.StartsWith(IsFolder ? Path.Combine(Source, string.Empty) : Source);
        }

        public string MappedPath(string path)
        {
            return path.Replace(Source + (IsFolder ? "\\" : string.Empty), Destination);
        }
    }
}
