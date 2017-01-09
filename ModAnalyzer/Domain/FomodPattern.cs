using System;
using System.Collections.Generic;
using System.Xml;

namespace ModAnalyzer.Domain {
    public class FomodPattern {
        public List<FomodFile> Files { get; }
        public List<FomodFlagDependency> Dependencies { get; }
        public bool HasComplexDependencies { get; }

        public FomodPattern(XmlNode node){ 
            XmlNode filesNode = node["files"];
            if (filesNode != null) {
                Files = FomodFile.FromNodes(filesNode.ChildNodes);
            }

            XmlNode dependenciesNode = node["dependencies"];
            if (dependenciesNode != null) {
                HasComplexDependencies = GetHasComplexDependencies(dependenciesNode);
                Dependencies = FomodFlagDependency.FromNodes(dependenciesNode.ChildNodes);
            }
        }

        public static List<FomodPattern> FromDocument(XmlDocument doc) {
            List<FomodPattern> patterns = new List<FomodPattern>();
            foreach (XmlNode node in doc.GetElementsByTagName("pattern")) {
                if (node.ParentNode.ParentNode.Name != "dependencyType") {
                    patterns.Add(new FomodPattern(node));
                }
            }
            return patterns;
        }

        private bool GetHasComplexDependencies(XmlNode dependenciesNode) {
            XmlAttribute operatorAttribute = dependenciesNode.Attributes["operator"];
            bool usesAndOperator = (operatorAttribute != null) && (operatorAttribute.Value == "And");
            bool hasMultipleDependencies = dependenciesNode.ChildNodes.Count > 1;
            bool hasNestedDependencies = GetHasNestedDependencies(dependenciesNode.ChildNodes);
            return hasNestedDependencies || (usesAndOperator && hasMultipleDependencies);
        }

        private bool GetHasNestedDependencies(XmlNodeList nodes) {
            foreach (XmlNode node in nodes) {
                if (node.Name.Equals("dependencies", StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }
    }
}