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

            var filesNode = node["files"];
            Files = filesNode == null ? new List<FomodFile>() : FomodFile.FromNodes(filesNode.ChildNodes);

            var flagsNode = node["conditionFlags"];
            Flags = flagsNode == null ? new List<FomodFlag>() : FomodFlag.FromNodes(flagsNode.ChildNodes);
        }

        public string GroupType { get; }
        public int SiblingCount { get; }
        public string Name { get; }
        public List<FomodFile> Files { get; }
        public List<FomodFlag> Flags { get; }

        public bool HasSiblings { get { return SiblingCount > 0; } }

        public bool IsDefault { get { return GroupType.Equals("SelectAll") || (!HasSiblings && (GroupType.Equals("SelectAtLeastOne") || GroupType.Equals("SelectExactlyOne"))); } }

        public static List<FomodPlugin> FromDocument(XmlDocument doc)
        {
            var plugins = new List<FomodPlugin>();
            foreach (XmlNode node in doc.GetElementsByTagName("plugin"))
                plugins.Add(new FomodPlugin(node));
            return plugins;
        }
    }
}
