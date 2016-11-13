using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ModAnalyzer.Domain
{
    /// <summary>
    ///     TODO
    /// </summary>
    public class FomodFlagDependency
    {
        public FomodFlagDependency(XmlNode node)
        {
            if (node.Attributes != null)
            {
                Flag = node.Attributes["flag"].Value;
                Value = node.Attributes["value"].Value;
            }
        }

        public string Flag { get; }
        public string Value { get; }

        public static List<FomodFlagDependency> FromNodes(XmlNodeList nodes)
        {
            return nodes.Cast<XmlNode>().Select(node => new FomodFlagDependency(node)).ToList();
        }
    }
}
