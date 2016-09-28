using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ModAnalyzer.Domain {
    public static class ModDump {
        /// <summary>
        /// Dynamically linking Delphi DLL for dumping plugins
        /// </summary>
        /// 
        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        static extern int LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        static extern IntPtr GetProcAddress(int hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        static extern bool FreeLibrary(int hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate void StartModDumpDelegate();
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate void GetBufferDelegate(StringBuilder str, int len);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate void FlushBufferDelegate();
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate void SetGameModeDelegate(int mode);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate bool PrepareDelegate(string FilePath);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate bool DumpDelegate();
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        delegate bool GetDumpResultDelegate(StringBuilder str, int len);

        public static bool loaded = false;
        public static int libHandle;
        const int BufferSize = 4 * 1024; // 4KB
        const int DumpSize = 4 * 1024 * 1024; // 4MB

        public static void LoadModDump() {
            if (loaded) return;
            const string dllName = "ModDumpLib.dll";

            libHandle = LoadLibrary(dllName);
            if (libHandle == 0)
                throw new Exception(string.Format("Could not load library \"{0}\"", dllName));

            loaded = true;
        }

        public static void UnloadModDump() {
            FreeLibrary(libHandle);
            loaded = false;
        }

        private static string GetFunctionException(string functionName) {
            return string.Format("Can't find function \"{0}\" in ModDumpLib", functionName);
        }

        private static Delegate GetDelegate<T>(string name) {
            var functionAddress = GetProcAddress(libHandle, name);
            if (functionAddress == IntPtr.Zero)
                throw new Exception(GetFunctionException(name));
            return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T));
        }

        public static void StartModDump() {
            var startModDump = (StartModDumpDelegate)GetDelegate<StartModDumpDelegate>("StartModDump");
            startModDump();
        }

        /*public static string GetBuffer() {
            var getBuffer = (GetBufferDelegate)GetDelegate<GetBufferDelegate>("GetBuffer");
            StringBuilder str = new StringBuilder();
            getBuffer(str, BufferSize);
            return str.ToString();
        }*/

        public static void GetBuffer(StringBuilder str, int len) {
            var getBuffer = (GetBufferDelegate)GetDelegate<GetBufferDelegate>("GetBuffer");
            getBuffer(str, len);
        }

        public static void FlushBuffer() {
            var flushBuffer = (FlushBufferDelegate)GetDelegate<FlushBufferDelegate>("FlushBuffer");
            flushBuffer();
        }

        public static void SetGameMode(int mode) {
            var setGameMode = (SetGameModeDelegate)GetDelegate<SetGameModeDelegate>("SetGameMode");
            setGameMode(mode);
        }

        public static bool Prepare(string FilePath) {
            var prepare = (PrepareDelegate)GetDelegate<PrepareDelegate>("Prepare");
            return prepare(FilePath);
        }

        public static bool Dump() {
            var dump = (DumpDelegate)GetDelegate<DumpDelegate>("Dump");
            return dump();
        }

        /*public static string GetDumpResult() {
            var getDumpResult = (GetDumpResultDelegate)GetDelegate<GetDumpResultDelegate>("GetBuffer");
            StringBuilder str = new StringBuilder();
            getDumpResult(str, DumpSize);
            return str.ToString();
        }*/

        public static bool GetDumpResult(StringBuilder str, int len) {
            var getDumpResult = (GetDumpResultDelegate)GetDelegate<GetDumpResultDelegate>("GetDumpResult");
            return getDumpResult(str, len);
        }
    }
}
