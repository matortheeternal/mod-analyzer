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

        public FomodFileNode(XmlNode node) {
            source = node["source"].Value;
            destination = node["destination"].Value;
            priority = Int32.Parse(node["priority"].Value);
        }
    }
}