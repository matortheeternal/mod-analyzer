﻿using System;
using System.Collections.Generic;
using System.Xml;

namespace ModAnalyzer.Domain.Fomod {
    public class FomodPlugin {
        public string GroupType { get; set; }
        public int SiblingCount { get; set; }
        public string Name { get; set; }
        public List<FomodFile> Files { get; }
        public List<FomodFlag> Flags { get; }
        public XmlNode VisibleNode { get; set; }

        public FomodPlugin(XmlNode node) {
            XmlAttribute nameAttribute = node.Attributes["name"];
            if (nameAttribute != null) {
                Name = nameAttribute.Value;
            }

            GetVisibleNode(node);
            XmlNode groupNode = node.ParentNode.ParentNode;
            if (groupNode != null) {
                // parse group information for determining whether or not the mod option is default
                XmlAttribute typeAttribute = groupNode.Attributes["type"];
                if (typeAttribute != null) {
                    GroupType = typeAttribute.Value;
                }
                SiblingCount = node.ParentNode.ChildNodes.Count - 1;

                // append group name to plugin name
                nameAttribute = groupNode.Attributes["name"];
                if (nameAttribute != null) {
                    Name = nameAttribute.Value + " - " + Name;
                }
            }

            XmlNode filesNode = node["files"];
            Files = (filesNode == null) ? new List<FomodFile>() : FomodFile.FromNodes(filesNode.ChildNodes);

            XmlNode flagsNode = node["conditionFlags"];
            Flags = (flagsNode == null) ? new List<FomodFlag>() : FomodFlag.FromNodes(flagsNode.ChildNodes);
        }

        public static List<FomodPlugin> FromDocument(XmlDocument doc) {
            List<FomodPlugin> plugins = new List<FomodPlugin>();
            foreach (XmlNode node in doc.GetElementsByTagName("plugin")) {
                plugins.Add(new FomodPlugin(node));
            }
            return plugins;
        }

        public bool HasSiblings() {
            return SiblingCount > 0;
        }

        public bool IsDefault() {
            return GroupType.Equals("SelectAll") || (!HasSiblings() &&
                (GroupType.Equals("SelectAtLeastOne") || GroupType.Equals("SelectExactlyOne")));
        }

        private XmlNode GetInstallStepNode(XmlNode node) {
            while (node != null) {
                if (node.Name.Equals("installStep")) return node;
                node = node.ParentNode;
            }
            return null;
        }

        private void GetVisibleNode(XmlNode node) {
            XmlNode InstallStep = GetInstallStepNode(node);
            if (InstallStep == null) return;
            foreach (XmlNode child in InstallStep.ChildNodes) {
                if (child.Name.Equals("visible")) {
                    VisibleNode = child;
                    break;
                }
            }
        }

        public Dictionary<string, string> GetFlags() {
            if (VisibleNode == null) return null;
            var flags = new Dictionary<string, string>();
            foreach (XmlNode child in VisibleNode.ChildNodes) {
                if (child.Name.Equals("flagDependency")) {
                    XmlAttribute flagName = child.Attributes["flag"];
                    XmlAttribute flagValue = child.Attributes["value"];
                    if (flagName == null || flagValue == null) continue;
                    flags.Add(flagName.Value, flagValue.Value);
                }
            }
            return flags;
        }
    }
}