using System.Collections.Generic;
using System.Xml;

namespace ModAnalyzer.Domain
{
    /// <summary>
    ///     TODO
    /// </summary>
    public class FomodPlugin
    {
        public FomodPlugin(XmlNode node)
        {
            if (node.ParentNode != null)
            {
                var groupNode = node.ParentNode.ParentNode;
                if (groupNode != null)
                {
                    if (groupNode.Attributes != null)
                    {
                        var typeAttribute = groupNode.Attributes["type"];
                        if (typeAttribute != null)
                            GroupType = typeAttribute.Value;
                    }
                    SiblingCount = groupNode.ChildNodes.Count - 1;
                }
            }

            if (node.Attributes != null)
            {
                var nameAttribute = node.Attributes["name"];
                if (nameAttribute != null)
                    Name = nameAttribute.Value;
            }

            XmlNode filesNode = node["files"];
            Files = filesNode == null ? new List<FomodFile>() : FomodFile.FromNodes(filesNode.ChildNodes);

            XmlNode flagsNode = node["conditionFlags"];
            Flags = flagsNode == null ? new List<FomodFlag>() : FomodFlag.FromNodes(flagsNode.ChildNodes);
        }

        public string GroupType { get; set; }
        public int SiblingCount { get; set; }
        public string Name { get; set; }
        public List<FomodFile> Files { get; }
        public List<FomodFlag> Flags { get; }

        public static List<FomodPlugin> FromDocument(XmlDocument doc)
        {
            var plugins = new List<FomodPlugin>();
            foreach (XmlNode node in doc.GetElementsByTagName("plugin"))
                plugins.Add(new FomodPlugin(node));
            return plugins;
        }

        public bool HasSiblings()
        {
            return SiblingCount > 0;
        }

        public bool IsDefault()
        {
            return GroupType.Equals("SelectAll") || (!HasSiblings() && (GroupType.Equals("SelectAtLeastOne") || GroupType.Equals("SelectExactlyOne")));
        }
    }
}
