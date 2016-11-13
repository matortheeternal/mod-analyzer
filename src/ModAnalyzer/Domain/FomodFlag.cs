using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ModAnalyzer.Domain
{
    /// <summary>
    ///     TODO
    /// </summary>
    public class FomodFlag
    {
        public FomodFlag(XmlNode node)
        {
            if (node.Attributes != null)
                Name = node.Attributes["name"].Value;
            Value = node.InnerText;
        }

        public string Name { get; }
        public string Value { get; }

        public static List<FomodFlag> FromNodes(XmlNodeList nodes)
        {
            return nodes.Cast<XmlNode>().Select(node => new FomodFlag(node)).ToList();
        }

        public bool Matches(FomodFlagDependency dependency)
        {
            return Name.Equals(dependency.Flag) && Value.Equals(dependency.Value);
        }
    }
}
