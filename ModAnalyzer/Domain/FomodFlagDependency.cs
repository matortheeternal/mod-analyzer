using System;
using System.Collections.Generic;
using System.Xml;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// TODO
    /// </summary>
    public class FomodFlagDependency {
        public string Flag { get; set; }
        public string Value { get; set; }

        public FomodFlagDependency(XmlNode node) {
            Flag = node.Attributes["flag"].Value;
            Value = node.Attributes["value"].Value;
        }

        public static List<FomodFlagDependency> FromNodes(XmlNodeList nodes) {
            List<FomodFlagDependency> dependencies = new List<FomodFlagDependency>();
            foreach (XmlNode node in nodes) {
                dependencies.Add(new FomodFlagDependency(node));
            }
            return dependencies;
        }
    }
}