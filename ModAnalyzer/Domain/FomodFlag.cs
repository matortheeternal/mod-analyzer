using System;
using System.Collections.Generic;
using System.Xml;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// TODO
    /// </summary>
    public class FomodFlag {
        public string Name { get; set; }
        public string Value { get; set; }

        public FomodFlag(XmlNode node) {
            Name = node.Attributes["name"].Value;
            Value = node.InnerText;
        }

        public static List<FomodFlag> FromNodes(XmlNodeList nodes) {
            List<FomodFlag> flags = new List<FomodFlag>();
            foreach (XmlNode node in nodes) {
                flags.Add(new FomodFlag(node));
            }
            return flags;
        }

        public bool Matches(FomodFlagDependency dependency) {
            return Name.Equals(dependency.Flag) && Value.Equals(dependency.Value);
        }
    }
}