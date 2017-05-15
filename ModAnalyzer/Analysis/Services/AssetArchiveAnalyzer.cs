using BA2Lib;
using libbsa;
using ModAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ModAnalyzer.Analysis.Services {
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

        public List<string> GetAssetPaths(string archivePath) {
            // extract BSA/BA2
            _backgroundWorker.ReportMessage(" ", false);
            string assetArchiveKey = string.Join(@"\", archivePath.Split('\\').Skip(2));
            _backgroundWorker.ReportMessage("Getting assets from " + archivePath + "...", true);

            // get the assets from the BSA/BA2
            List<string> assets;
            string archiveExt = Path.GetExtension(archivePath);
            if (archiveExt.Equals(".bsa", StringComparison.InvariantCultureIgnoreCase)) {
                assets = GetBSAAssets(archivePath).Select(asset => PrepareAssetPath(assetArchiveKey, asset)).ToList();
            } else {
                assets = GetBA2Assets(archivePath).Select(asset => PrepareAssetPath(assetArchiveKey, asset)).ToList();
            }

            // report assets to the user
            assets.ForEach(asset => _backgroundWorker.ReportMessage(asset, false));
            return assets;
        }

        private string[] GetBSAAssets(string bsaPath) {
            string[] entries = null;

            // create BSAManager and use it to get the assets from the BSA
            BSANET bsaManager = new BSANET();
            if (bsaManager.bsa_open(bsaPath) == 0) {
                entries = bsaManager.bsa_get_assets(".*");
            }
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