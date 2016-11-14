﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// Represents a logical component of a mod, e.g. the base mod, a patch, or a fomod option.
    /// </summary>
    public class ModOption {

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }
        [JsonProperty(PropertyName = "md5_hash")]
        public string MD5Hash { get; set; }
        [JsonProperty(PropertyName = "default")]
        public bool Default { get; set; }
        [JsonProperty(PropertyName = "is_fomod_option")]
        public bool IsFomodOption { get; set; }
        [JsonProperty(PropertyName = "assets")]
        public List<string> Assets { get; set; }
        [JsonProperty(PropertyName = "plugins")]
        public List<PluginDump> Plugins { get; set; }
        [JsonIgnore]
        public string SourceFilePath { get; set; }

        public ModOption() {
            Assets = new List<string>();
            Plugins = new List<PluginDump>();
            Size = 0;
        }

        public ModOption(string Name, bool Default, bool IsFomodOption) {
            this.Name = Name;
            this.Default = Default;
            this.IsFomodOption = IsFomodOption;
            Assets = new List<string>();
            Plugins = new List<PluginDump>();
            Size = 0;
        }

        public void GetMD5Hash() {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(SourceFilePath)) {
                    MD5Hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        public static bool IsEmpty(ModOption option) {
            return (option.Assets.Count == 0) && (option.Plugins.Count == 0);
        }
    }
}