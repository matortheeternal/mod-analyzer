using System;
using System.Collections.Generic;
using System.Xml;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// TODO
    /// </summary>
    public class FomodPlugin {
        public string GroupType { get; set; }
        public int SiblingCount { get; set; }
        public string Name { get; set; }
        public List<FomodFile> Files { get; }
        public List<FomodFlag> Flags { get; }

        public FomodPlugin(XmlNode node) {
            XmlNode groupNode = node.ParentNode.ParentNode;
            if (groupNode != null) {
                XmlAttribute typeAttribute = groupNode.Attributes["type"];
                if (typeAttribute != null) {
                    GroupType = typeAttribute.Value;
                }
                SiblingCount = groupNode.ChildNodes.Count - 1;
            }

            XmlAttribute nameAttribute = node.Attributes["name"];
            if (nameAttribute != null) {
                Name = nameAttribute.Value;
            }

            XmlNode filesNode = node["files"];
            if (filesNode != null) {
                Files = FomodFile.FromNodes(filesNode.ChildNodes);
            }

            XmlNode flagsNode = node["conditionFlags"];
            if (flagsNode != null) {
                Flags = FomodFlag.FromNodes(flagsNode.ChildNodes);
            }
        }

        public static List<FomodPlugin> FromDocument(XmlDocument doc) {
            List<FomodPlugin> plugins = new List<FomodPlugin>();
            foreach (XmlNode node in doc.GetElementsByTagName("plugin")) {
                plugins.Add(new FomodPlugin(node));
            }
            return plugins;
        }

        public bool HasSiblings() {
            return SiblingCount > 0;
        }

        public bool IsDefault() {
            return GroupType.Equals("SelectAll") || (!HasSiblings() && 
                (GroupType.Equals("SelectAtLeastOne") || GroupType.Equals("SelectExactlyOne")));
        }
    }
}