using ModAnalyzer.Utils;
using SharpCompress.Archive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModAnalyzer.Domain
{
    public class ModAnalyzerService
    {
        private readonly ModAnalysis _modAnalysis;
        private readonly AssetArchiveAnalyzer _assetArchiveAnalyzer;
        private readonly PluginAnalyzer _pluginAnalyzer;
        private List<IArchiveEntry> _plugins;

        public event EventHandler<ProgressMessageChangedEventArgs> ProgressMessageChanged;

        public void RaiseProgressMessageChanged(string progressMessage)
        {
            ProgressMessageChanged?.Invoke(this, new ProgressMessageChangedEventArgs(progressMessage));
        }

        public event EventHandler<LogMessageAddedEventArgs> LogMessageAdded;

        public void RaiseLogMessageAdded(string logMessage)
        {
            LogMessageAdded?.Invoke(this, new LogMessageAddedEventArgs(logMessage);
        }

        public ModAnalyzerService()
        {
            _modAnalysis = new ModAnalysis();

            _assetArchiveAnalyzer = new AssetArchiveAnalyzer();
            _assetArchiveAnalyzer.ProgressMessageChanged += _assetArchiveAnalyzer_ProgressMessageChanged;

            _pluginAnalyzer = new PluginAnalyzer();
        }

        private void _assetArchiveAnalyzer_ProgressMessageChanged(object sender, ProgressMessageChangedEventArgs e)
        {
            RaiseProgressMessageChanged(e.ProgressMessage);
        }

        public ModAnalysis AnalyzeMod(string path)
        {
            using (IArchive archive = ArchiveFactory.Open(@path))
            {
                RaiseProgressMessageChanged("Analyzing archive entries...");

                foreach (IArchiveEntry modArchiveEntry in archive.Entries)
                {
                    if (modArchiveEntry.IsDirectory)
                        continue;

                    ProcessModArchiveEntry(modArchiveEntry);
                }

                return _modAnalysis;
            }
        }

        private void ProcessModArchiveEntry(IArchiveEntry modArchiveEntry)
        {
            string entryPath = modArchiveEntry.GetEntryPath();
            _modAnalysis.assets.Add(entryPath);

            RaiseLogMessageAdded(entryPath);

            switch (modArchiveEntry.GetEntryExtesion())
            {
                case ".BA2":
                case ".BSA":
                    AnalyzeAssetArchive(modArchiveEntry);                    
                    break;
                case ".ESP":
                case ".ESM":
                    // Don't analyze the plugin yet. All plugins will be analyzed after asset archives.
                    _plugins.Add(modArchiveEntry);
                    break;
            }

            if (_plugins.Any())
                AnalyzePlugins();
        }

        private void AnalyzeAssetArchive(IArchiveEntry assetArchive)
        {
            RaiseProgressMessageChanged("Extracting " + assetArchive.GetEntryExtesion() + " at " + assetArchive.GetEntryPath());
            List<string> assets = _assetArchiveAnalyzer.Analyze(assetArchive);
            _modAnalysis.assets.AddRange(assets);

            foreach (string asset in assets)
                RaiseLogMessageAdded(asset);
        }

        private void AnalyzePlugins()
        {
            AddLogMessage("Extracting and analyzing plugins...");

            foreach (IArchiveEntry plugin in _plugins)
            {
                try
                {
                    ProcessPlugin(plugin);
                }
                catch (Exception e)
                {
                    AddLogMessage("Failed to analyze plugin.");
                    AddLogMessage("Exception:" + e.Message);
                }
            }
        }
    }
}
