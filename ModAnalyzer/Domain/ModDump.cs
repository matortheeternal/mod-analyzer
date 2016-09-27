using System.Runtime.InteropServices;
using System.Text;

namespace ModAnalyzer.Domain {
    public static class ModDump {
        /// <summary>
        /// Linking Delphi DLL for dumping plugins
        /// </summary>
        public static bool started;

        [DllImport(@"ModDumpLib.dll")]
        public static extern void StartModDump();
        [DllImport(@"ModDumpLib.dll")]
        public static extern void EndModDump();
        [DllImport(@"ModDumpLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void GetBuffer(StringBuilder str, int len);
        [DllImport(@"ModDumpLib.dll")]
        public static extern void FlushBuffer();
        [DllImport(@"ModDumpLib.dll")]
        public static extern void SetGameMode(int mode);
        [DllImport(@"ModDumpLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Prepare(string FilePath);
        [DllImport(@"ModDumpLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Dump();
        [DllImport(@"ModDumpLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool GetDumpResult(StringBuilder str, int len);
    }
}
