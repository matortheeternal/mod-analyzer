using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BA2Lib;
using libbsa;
using ModAnalyzer.Utils;
using SharpCompress.Archive;

namespace ModAnalyzer.Domain
{
    internal class AssetArchiveAnalyzer
    {
        private readonly BackgroundWorker _backgroundWorker;

        public AssetArchiveAnalyzer(BackgroundWorker backgroundWorker)
        {
            _backgroundWorker = backgroundWorker;
            Directory.CreateDirectory(@".\bsas");
        }

        private static string PrepareAssetPath(string archiveKey, string assetPath)
        {
            if (assetPath.StartsWith(@".\"))
                assetPath = assetPath.Remove(0, 2);
            return Path.Combine(archiveKey, assetPath);
        }

        public List<string> GetAssetPaths(IArchiveEntry assetArchive)
        {
            // extract BSA/BA2
            _backgroundWorker.ReportMessage(" ");
            var extractedArchivePath = ExtractArchive(assetArchive);
            _backgroundWorker.ReportMessage("Getting assets from " + extractedArchivePath + "...", true);
            // get the assets from the BSA/BA2
            var assets = assetArchive.GetEntryExtension().Equals(".bsa", StringComparison.OrdinalIgnoreCase) ? GetBsaAssets(extractedArchivePath) : GetBa2Assets(extractedArchivePath);
            var prepared = assets.Select(asset => PrepareAssetPath(assetArchive.Key, asset)).ToList();
            // report assets to the user
            prepared.ForEach(asset => _backgroundWorker.ReportMessage(asset));
            return prepared;
        }

        private string ExtractArchive(IArchiveEntry assetArchive)
        {
            // prepare helper variables
            var archivePath = assetArchive.GetEntryPath();
            var archiveName = Path.GetFileName(archivePath);
            if (string.IsNullOrEmpty(archiveName))
                throw new ArgumentNullException(nameof(archiveName));
            var outputPath = Path.Combine("bsas", archiveName);
            var ext = assetArchive.GetEntryExtension().Remove(0, 1);

            // return if the archive has already been extracted
            if (File.Exists(outputPath))
                return outputPath;

            // extract archive and report to the user
            _backgroundWorker.ReportMessage("Extracting " + ext + " at " + archivePath + "...", true);
            assetArchive.WriteToDirectory(@".\bsas");
            _backgroundWorker.ReportMessage(archivePath + " extracted, analyzing entries...", true);

            // return the path of the extracted archive file
            return outputPath;
        }

        private static IEnumerable<string> GetBsaAssets(string bsaPath)
        {
            var bsaManager = new BSANET(); // create BSAManager and use it to get the assets from the BSA
            try
            {
                if (bsaManager.bsa_open(bsaPath) == 0)
                    return bsaManager.bsa_get_assets(".*"); // return the entries we found
                return Enumerable.Empty<string>();
            }
            finally
            {
                bsaManager.bsa_close();
            }
        }

        private static IEnumerable<string> GetBa2Assets(string ba2Path)
        {
            using (var ba2Manager = new BA2NET()) // create ba2Manager and use it to get the assets from the BA2
            {
                if (ba2Manager.Open(ba2Path))
                    return ba2Manager.GetNameTable(); // return the entries we found
                return Enumerable.Empty<string>();
            }
        }
    }
}
