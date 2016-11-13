using System.Collections.Generic;
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

        public string Name { get; set; }
        public string Value { get; set; }

        public static List<FomodFlag> FromNodes(XmlNodeList nodes)
        {
            var flags = new List<FomodFlag>();
            foreach (XmlNode node in nodes)
                flags.Add(new FomodFlag(node));
            return flags;
        }

        public bool Matches(FomodFlagDependency dependency)
        {
            return Name.Equals(dependency.Flag) && Value.Equals(dependency.Value);
        }
    }
}
