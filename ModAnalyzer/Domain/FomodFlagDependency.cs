using System.Collections.Generic;
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

        public string Flag { get; set; }
        public string Value { get; set; }

        public static List<FomodFlagDependency> FromNodes(XmlNodeList nodes)
        {
            var dependencies = new List<FomodFlagDependency>();
            foreach (XmlNode node in nodes)
                dependencies.Add(new FomodFlagDependency(node));
            return dependencies;
        }
    }
}
