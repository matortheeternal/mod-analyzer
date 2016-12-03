﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using SharpCompress.Archive;
using System.Linq;
using ModAnalyzer.Utils;

namespace ModAnalyzer.Domain {
    /// <summary>
    /// Represents a logical component of a mod, e.g. the base mod, a patch, or a fomod option.
    /// </summary>
    public class ModOption {

        // PROPERTIES
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }
        [JsonProperty(PropertyName = "md5_hash")]
        public string MD5Hash { get; set; }
        [JsonProperty(PropertyName = "default")]
        public bool Default { get; set; }
        [JsonProperty(PropertyName = "is_installer_option")]
        public bool IsInstallerOption { get; set; }
        [JsonProperty(PropertyName = "assets")]
        public List<string> Assets { get; set; }
        [JsonProperty(PropertyName = "plugins")]
        public List<PluginDump> Plugins { get; set; }
        [JsonIgnore]
        public string SourceFilePath { get; set; }
        [JsonIgnore]
        public IArchive Archive { get; set; }
        [JsonIgnore]
        public bool IsBainArchive { get; set; }
        [JsonIgnore]
        public bool IsFomodArchive { get; set; }
        [JsonIgnore]
        public string FomodConfigPath { get; set; }
        [JsonIgnore]
        public string BaseInstallerPath { get; set; }


        // CONSTRUCTORS
        public ModOption() {
            Assets = new List<string>();
            Plugins = new List<PluginDump>();
            Size = 0;
        }

        public ModOption(string Name, string SourceFilePath, bool Default) {
            this.Name = Name;
            this.Default = Default;
            this.SourceFilePath = SourceFilePath;
            Assets = new List<string>();
            Plugins = new List<PluginDump>();
            Archive = ArchiveFactory.Open(SourceFilePath);
            Size = Archive.TotalUncompressSize;
            GetInstallerType();
        }

        public ModOption(string Name, bool Default, bool IsInstallerOption) {
            this.Name = Name;
            this.Default = Default;
            this.IsInstallerOption = IsInstallerOption;
            Assets = new List<string>();
            Plugins = new List<PluginDump>();
            Size = 0;
        }

        // HELPER METHODS
        public void GetMD5Hash() {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(SourceFilePath)) {
                    MD5Hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        public void GetInstallerType() {
            IsInstallerOption = GetIsFomodArchive() || GetIsBainArchive();
        }

        public static bool IsEmpty(ModOption option) {
            return (option.Assets.Count == 0) && (option.Plugins.Count == 0);
        }

        public string MapArchiveAssetPath(string archiveAssetPath, string archiveFileName, string archivePath) {
            int index = archiveAssetPath.IndexOf(archiveFileName);
            if (index > -1) {
                return archivePath + archiveAssetPath.Substring(index + archiveFileName.Length);
            } else {
                return archiveAssetPath;
            }
        }

        public void AddArchiveAssetPaths(string archiveFileName, List<string> archiveAssetPaths) {
            string archivePath = Assets.Find(asset => Path.GetFileName(asset) == archiveFileName);
            if (archivePath == null) return;
            foreach (string archiveAssetPath in archiveAssetPaths) {
                string mappedPath = MapArchiveAssetPath(archiveAssetPath, archiveFileName, archivePath);
                Assets.Add(mappedPath);
            }
        }
        
        // BAIN ARCHIVE HANDLING
        public bool GetIsBainArchive() {
            string BaseBainPath = BainArchiveService.GetBasePath(Archive);
            if (BaseBainPath != null) {
                IsBainArchive = true;
                BaseInstallerPath = BaseBainPath;
            }

            return IsBainArchive;
        }

        public List<string> GetValidBainDirectories() {
            return BainArchiveService.GetValidDirectories(Archive, BaseInstallerPath);
        }

        // FOMOD ARCHIVE HANDLING
        private bool GetIsFomodArchive() {
            IArchiveEntry FomodConfigEntry = FomodArchiveService.GetFomodConfig(Archive);
            if (FomodConfigEntry != null) {
                IsFomodArchive = true;
                FomodConfigPath = FomodConfigEntry.GetPath();
                BaseInstallerPath = FomodArchiveService.GetBasePath(FomodConfigPath);
            }

            return IsFomodArchive;
        }
    }
}