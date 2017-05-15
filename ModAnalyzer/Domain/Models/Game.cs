using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;

namespace ModAnalyzer.Domain.Models {
    public class Game {
        public string abbrName { get; set; }
        public string longName { get; set; }
        public string gameName { get; set; }
        public string regName { get; set; }
        public int gameMode { get; set; }
        public int gameId { get; set; }
        public string appName { get; set; }
        public string exeName { get; set; }
        public string appIDs { get; set; }

        public string GetGamePath() {
            List<string> keys = new List<string>() {
                Path.Combine("Bethesda Softworks", regName, "Installed Path"),
                Path.Combine(@"Wow6432Node\Bethesda Softworks", regName, "Installed Path")
            };
            foreach (string appID in appIDs.Split(',')) {
                keys.Add(string.Format(@"Microsoft\Windows\CurrentVersion\Uninstall\Steam App {0}\InstallLocation", appID));
                keys.Add(string.Format(@"Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App {0}\InstallLocation", appID));
            }
            RegistryKey localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            foreach (string key in keys) {
                string keyName = Path.Combine("SOFTWARE", Path.GetDirectoryName(key));
                string valueName = Path.GetFileName(key);
                RegistryKey regKey = localKey.OpenSubKey(keyName, false);
                if (regKey != null) return (string)regKey.GetValue(valueName);
            }
            return "";
        }
    }
}
