using System.Runtime.InteropServices;
using System.Text;

namespace ModAnalyzer.Wrappers
{
    public static class ModDumpWrapper
    {
        /// <summary>
        ///     Linking Delphi DLL for dumping plugins
        /// </summary>
        public static bool Started;

        [DllImport("ModDumpLib")]
        public static extern void StartModDump();

        [DllImport("ModDumpLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void GetBuffer(StringBuilder str, int len);

        [DllImport("ModDumpLib")]
        public static extern void FlushBuffer();

        [DllImport("ModDumpLib")]
        public static extern void SetGameMode(int mode);

        [DllImport("ModDumpLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Prepare(string filePath);

        [DllImport("ModDumpLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool Dump();

        [DllImport("ModDumpLib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern bool GetDumpResult(StringBuilder str, int len);
    }
}
