using System.Runtime.InteropServices;
using System.Text;

namespace ModAnalyzer
{
    public static class ModDump
    {
        /// <summary>
        /// Linking Delphi DLL for dumping plugins
        /// </summary>
        [DllImport(@"lib\ModDumpLib.dll")]
        public static extern void StartModDump();
        [DllImport(@"lib\ModDumpLib.dll")]
        public static extern void EndModDump();
        [DllImport(@"lib\ModDumpLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void GetBuffer(StringBuilder str, int len);
        [DllImport(@"lib\ModDumpLib.dll")]
        public static extern void FlushBuffer();
        [DllImport(@"lib\ModDumpLib.dll")]
        public static extern void SetGameMode(int mode);
        [DllImport(@"lib\ModDumpLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Prepare(string FilePath);
        [DllImport(@"lib\ModDumpLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Dump(StringBuilder str, int len);
    }
}
