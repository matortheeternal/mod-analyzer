using BA2Lib;
using libbsa;
using ModAnalyzer.Utils;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ModAnalyzer.Domain {
    internal class AssetArchiveAnalyzer {
        private readonly BackgroundWorker _backgroundWorker;

        public AssetArchiveAnalyzer(BackgroundWorker backgroundWorker) {
            _backgroundWorker = backgroundWorker;

            Directory.CreateDirectory(@".\bsas");
        }

        private string PrepareAssetPath(string archiveKey, string assetPath) {
            if (assetPath.StartsWith(@".\")) {
                assetPath = assetPath.Remove(0, 2);
            }
            return Path.Combine(archiveKey, assetPath);
        }

        public List<string> GetAssetPaths(IArchiveEntry assetArchive) {
            // extract BSA/BA2
            _backgroundWorker.ReportMessage(" ", false);
            string extractedArchivePath = ExtractArchive(assetArchive);
            _backgroundWorker.ReportMessage("Getting assets from " + extractedArchivePath + "...", true);

            // get the assets from the BSA/BA2
            List<string> assets;
            if (assetArchive.GetEntryExtension().Equals(".bsa", StringComparison.InvariantCultureIgnoreCase))
                assets = GetBSAAssets(extractedArchivePath).Select(asset => PrepareAssetPath(assetArchive.Key, asset)).ToList();
            else
                assets = GetBA2Assets(extractedArchivePath).Select(asset => PrepareAssetPath(assetArchive.Key, asset)).ToList();

            // report assets to the user
            assets.ForEach(asset => _backgroundWorker.ReportMessage(asset, false));
            return assets;
        }

        private string ExtractArchive(IArchiveEntry assetArchive) {
            // prepare helper variables
            string archivePath = assetArchive.GetEntryPath();
            string outputPath = Path.Combine("bsas", Path.GetFileName(archivePath));
            string ext = assetArchive.GetEntryExtension().Remove(0, 1);

            // return if the archive has already been extracted
            if (File.Exists(outputPath)) {
                return outputPath;
            }

            // extract archive and report to the user
            _backgroundWorker.ReportMessage("Extracting " + ext + " at " + archivePath + "...", true);
            assetArchive.WriteToDirectory(@".\bsas", ExtractOptions.Overwrite);
            _backgroundWorker.ReportMessage(archivePath + " extracted, analyzing entries...", true);

            // return the path of the extracted archive file
            return outputPath;
        }

        private string[] GetBSAAssets(string bsaPath) {
            string[] entries = null;

            // create BSAManager and use it to get the assets from the BSA
            BSANET bsaManager = new BSANET();
            if (bsaManager.bsa_open(bsaPath) == 0)
                entries = bsaManager.bsa_get_assets(".*");
            bsaManager.bsa_close();

            // return the entries we found
            return entries;
        }

        private string[] GetBA2Assets(string ba2Path) {
            string[] entries = null;

            // create ba2Manager and use it to get the assets from the BA2
            using (BA2NET ba2Manager = new BA2NET()) {
                if (ba2Manager.Open(ba2Path))
                    entries = ba2Manager.GetNameTable();
            }

            // return the entries we found
            return entries;
        }
    }
}