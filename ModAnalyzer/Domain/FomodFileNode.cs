using System;
using System.Xml;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// Used to store information on a fomod file or folder node
    /// </summary>
    public class FomodFileNode {
        public string source { get; set; }
        public string destination { get; set; }
        public int priority { get; set; }

        public FomodFileNode(XmlNode node)
        {
            source = node.Attributes["source"].Value;
            destination = node.Attributes["destination"].Value;
            if (node.Attributes["priority"] != null)
            {
                priority = Int32.Parse(node.Attributes["priority"].Value);
            }
        }
    }
}