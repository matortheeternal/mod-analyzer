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

        public List<string> GetAssets(IArchiveEntry assetArchive) {
            ExtractArchive(assetArchive);

            _backgroundWorker.ReportMessage("Getting assets from " + assetArchive.GetEntryPath() + "...", true);

            string extractedArchivePath = Path.Combine("bsas", assetArchive.Key);

            List<string> assets;

            if (assetArchive.GetEntryExtension().Equals(".bsa", StringComparison.InvariantCultureIgnoreCase))
                assets = GetBSAAssets(extractedArchivePath).Select(asset => Path.Combine(assetArchive.Key, asset)).ToList();
            else
                assets = GetBA2Assets(extractedArchivePath).Select(asset => Path.Combine(assetArchive.Key, asset)).ToList();

            assets.ForEach(asset => _backgroundWorker.ReportMessage(asset, false));

            return assets;
        }

        private void ExtractArchive(IArchiveEntry assetArchive) {
            _backgroundWorker.ReportProgress(0, MessageReportedEventArgsFactory.CreateProgressMessageEventArgs("Extracting " + assetArchive.GetEntryExtension() + " at " + assetArchive.GetEntryPath()));

            assetArchive.WriteToDirectory(@".\bsas", ExtractOptions.Overwrite);

            _backgroundWorker.ReportMessage(assetArchive.GetEntryExtension() + " extracted, analyzing entries...", true);
        }

        private string[] GetBSAAssets(string bsaPath) {
            string[] entries = null;

            BSANET bsaManager = new BSANET();

            if (bsaManager.bsa_open(bsaPath) == 0)
                entries = bsaManager.bsa_get_assets(".*");

            bsaManager.bsa_close();

            return entries;
        }

        private string[] GetBA2Assets(string ba2Path) {
            string[] entries = null;

            using (BA2NET ba2Manager = new BA2NET()) {
                if (ba2Manager.Open(ba2Path))
                    entries = ba2Manager.GetNameTable();
            }

            return entries;
        }
    }
}