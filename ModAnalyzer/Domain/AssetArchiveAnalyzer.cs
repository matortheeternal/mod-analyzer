using System.ComponentModel;
using System.IO;

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
    }
}