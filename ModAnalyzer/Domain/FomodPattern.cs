using System;
using System.Collections.Generic;
using System.Xml;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// TODO
    /// </summary>
    public class FomodPattern {
        public List<FomodFile> Files { get; }
        public List<FomodFlagDependency> Dependencies { get; }

        public FomodPattern(XmlNode node) {
            XmlNode filesNode = node["files"];
            if (filesNode != null) {
                Files = FomodFile.FromNodes(filesNode.ChildNodes);
            }

            XmlNode dependenciesNode = node["dependencies"];
            if (dependenciesNode != null) {
                Dependencies = FomodFlagDependency.FromNodes(dependenciesNode.ChildNodes);
            }
        }

        public static List<FomodPattern> FromDocument(XmlDocument doc) {
            List<FomodPattern> patterns = new List<FomodPattern>();
            foreach (XmlNode node in doc.GetElementsByTagName("pattern")) {
                patterns.Add(new FomodPattern(node));
            }
            return patterns;
        }
    }
}