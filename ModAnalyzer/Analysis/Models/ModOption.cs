using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using SevenZipExtractor;
using ModAnalyzer.Domain.Services;
using ModAnalyzer.Analysis.Services;

namespace ModAnalyzer.Analysis.Models {
    /// <summary>
    /// Represents a logical component of a mod, e.g. the base mod, a patch, or a fomod option.
    /// </summary>
    public class ModOption {
        [JsonIgnore]
        private ArchiveFile _archive;

        // PROPERTIES
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }
        //[JsonProperty(PropertyName = "unpacked_size")]
        [JsonIgnore]
        public long UnpackedSize { get; set; }
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
        public ArchiveFile Archive {
            get {
                if (_archive == null) {
                    _archive = new ArchiveFile(SourceFilePath);
                }
                return _archive;
            }
        }
        [JsonIgnore]
        public bool IsArchiveInstaller { get; set; }
        [JsonIgnore]
        public bool IsBainArchive { get; set; }
        [JsonIgnore]
        public bool IsFlexArchive { get; set; }
        [JsonIgnore]
        public bool IsFomodArchive { get; set; }
        [JsonIgnore]
        public string FomodConfigPath { get; set; }
        [JsonIgnore]
        public string BaseInstallerPath { get; set; }
        [JsonIgnore]
        public Dictionary<Entry,string> ExtractPaths { get; set; }
        [JsonIgnore]
        public List<string> PluginPaths { get; set; }
        [JsonIgnore]
        public List<string> ArchivePaths { get; set; }
        [JsonIgnore]
        public bool HasFilesToExtract {
            get {
                return ExtractPaths.Any(x => x.Value != null && !File.Exists(x.Value));
            }
        }


        // CONSTRUCTORS
        public ModOption() {
            Size = 0;
            CreateLists();
        }

        public ModOption(string Name, string SourceFilePath, bool Default) {
            this.Name = Name;
            this.Default = Default;
            this.SourceFilePath = SourceFilePath;
            Size = (new FileInfo(SourceFilePath)).Length;
            CreateLists();
            GetInstallerType();
        }

        public ModOption(string Name, bool Default, bool IsInstallerOption) {
            this.Name = Name;
            this.Default = Default;
            this.IsInstallerOption = IsInstallerOption;
            Size = 0;
            CreateLists();
        }

        // HELPER METHODS
        public void CreateLists() {
            Assets = new List<string>();
            Plugins = new List<PluginDump>();
            ExtractPaths = new Dictionary<Entry, string>();
            PluginPaths = new List<string>();
            ArchivePaths = new List<string>();
        }

        public void GetMD5Hash() {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(SourceFilePath)) {
                    MD5Hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        public void GetInstallerType() {
            IsArchiveInstaller = GetIsFomodArchive() || GetIsBainArchive();
        }

        public static bool IsEmpty(ModOption option) {
            return (option.Assets.Count == 0) && (option.Plugins.Count == 0);
        }

        public string MapArchiveAssetPath(string archiveAssetPath, string archiveFileName, string archivePath) {
            int index = archiveAssetPath.IndexOf(archiveFileName);
            if (index > -1) {
                return archivePath + archiveAssetPath.Substring(index + archiveFileName.Length);
            }
            else {
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

        public void AddPluginDump(PluginDump dump) {
            Plugins.Add(dump);
        }

        public string GetExtractedEntryPath(Entry entry) {
            return Path.Combine("extracted", Path.GetFileName(SourceFilePath), entry.FileName);
        }

        public void SetExtractPath(Entry entry, string destinationPath) {
            if (destinationPath != null) {
                ExtractPaths.Add(entry, destinationPath);
                if (ArchiveHelpers.IsPlugin(destinationPath)) {
                    PluginPaths.Add(destinationPath);
                } else if (ArchiveHelpers.IsArchive(destinationPath)) {
                    PluginPaths.Add(destinationPath);
                }
            }
        }

        public string GetExtractPath(Entry entry) {
            if (ExtractPaths.ContainsKey(entry)) {
                string path = ExtractPaths[entry];
                return File.Exists(path) ? null : path;
            } else {
                return null;
            }
        }

        public void CloseArchive() {
            if (_archive != null) {
                _archive.Dispose();
                _archive = null;
            }
        }

        public List<string> GetInstallerDirectories(bool data = true) {
            return BainArchiveService.GetDirectories(Archive, BaseInstallerPath, data);
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

        // FOMOD ARCHIVE HANDLING
        private bool GetIsFomodArchive() {
            Entry FomodConfigEntry = FomodArchiveService.GetFomodConfig(Archive);
            if (FomodConfigEntry != null) {
                IsFomodArchive = true;
                FomodConfigPath = FomodConfigEntry.FileName;
                BaseInstallerPath = FomodArchiveService.GetBasePath(FomodConfigPath);
            }
            return IsFomodArchive;
        }
    }
}