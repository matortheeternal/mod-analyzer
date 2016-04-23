using BA2Lib;
using libbsa;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ModAssetMapper.Services
{
    public class AssetArchiveService
    {
        private readonly BA2NET _ba2;
        private readonly BSANET _bsa;

        public AssetArchiveService(BA2NET ba2, BSANET bsa)
        {
            _ba2 = ba2;
            _bsa = bsa;
        }

        public List<string> GetEntryMap(string path)
        {
            List<string> entryMap = new List<string>();

            // load the archive
            var archive = ArchiveFactory.Open(path);

            // ProgressMessage("Analyzing archive entries...");

            // loop through entries in archive
            foreach (var entry in archive.Entries)
            {
                if (entry.IsDirectory)
                    continue;

                string entryPath = entry.Key;
                entryMap.Add(entryPath);

                string extension = Path.GetExtension(entryPath);
                if (string.Equals(extension, ".ba2", StringComparison.OrdinalIgnoreCase))
                {
                    entryMap.AddRange(GetBA2Entries(entry));
                    //ProgressMessage("Extracting BA2 at " + entryPath);
                    //ProgressMessage("Analyzing archive entries...");
                }
                else if (String.Equals(extension, ".bsa", StringComparison.OrdinalIgnoreCase))
                {
                    //ProgressMessage("Extracting BSA at " + entryPath);
                    entryMap.AddRange(GetBSAEntries(entry));
                    //ProgressMessage("Analyzing archive entries...");
                }
            }

            return entryMap;
        }

        private List<string> GetBA2Entries(IArchiveEntry entry)
        {
            List<string> ba2Entries = new List<string>();

            entry.WriteToDirectory(@".\\bsas", ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
            //ProgressMessage("BSA extracted, Analyzing entries...");
            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bsaPath = rootPath + "\\bsas\\" + entry.Key;
            if (_ba2.Open(bsaPath))
            {
                string[] entries = _ba2.GetNameTable();
                for (int i = 0; i < entries.Length; i++)
                {
                    String entryPath = entry.Key + "\\" + entries[i];
                    //LogMessage(entryPath);
                    ba2Entries.Add(entryPath);
                }
            }

            return ba2Entries;
        }

        private List<string> GetBSAEntries(IArchiveEntry entry)
        {
            List<string> ba2Entries = new List<string>();

            entry.WriteToDirectory(@".\\bsas", ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
            string rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bsaPath = rootPath + "\\bsas\\" + entry.Key;
            if (_bsa.bsa_open(bsaPath) == 0)
            {
                string[] entries = _bsa.bsa_get_assets(".*");
                for (int i = 0; i < entries.Length; i++)
                {
                    String entryPath = entry.Key + "\\" + entries[i];
                    // LogMessage(entryPath);
                    ba2Entries.Add(entryPath);
                }
            }

            return ba2Entries;
        }
    }
}
