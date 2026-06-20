using System.Runtime.InteropServices;

namespace NRTX.Optimizer.Core.Native;

public static class NativeMethods
{
    // ==========================================
    // MEMORY MANAGEMENT (Psapi / Kernel32)
    // ==========================================
    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool EmptyWorkingSet(IntPtr hProcess);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr dwMinimumWorkingSetSize, IntPtr dwMaximumWorkingSetSize);

    [StructLayout(LayoutKind.Sequential)]
    public struct PERFORMANCE_INFORMATION
    {
        public uint cb;
        public UIntPtr CommitTotal;
        public UIntPtr CommitLimit;
        public UIntPtr CommitPeak;
        public UIntPtr PhysicalTotal;
        public UIntPtr PhysicalAvailable;
        public UIntPtr SystemCache;
        public UIntPtr KernelTotal;
        public UIntPtr KernelPaged;
        public UIntPtr KernelNonpaged;
        public UIntPtr PageSize;
        public uint HandleCount;
        public uint ProcessCount;
        public uint ThreadCount;
    }

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool GetPerformanceInfo(out PERFORMANCE_INFORMATION pPerformanceInformation, uint cb);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    // ==========================================
    // DNS CACHE FLUSH (Dnsapi.dll)
    // ==========================================
    [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache", SetLastError = true)]
    public static extern int DnsFlushResolverCache();

    // ==========================================
    // POWER SCHEMES (Powrprof.dll)
    // ==========================================
    [DllImport("powrprof.dll", SetLastError = true)]
    public static extern uint PowerSetActiveScheme(IntPtr UserRootPowerKey, ref Guid SchemeGuid);

    [DllImport("powrprof.dll", SetLastError = true)]
    public static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, out IntPtr pActivePolicyGuid);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr LocalFree(IntPtr hMem);

    // GUIDs for Standard Power Schemes
    public static readonly Guid GUID_BALANCED = new("381b4222-f694-41f0-9685-ff5bb260df2e");
    public static readonly Guid GUID_HIGH_PERFORMANCE = new("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
    public static readonly Guid GUID_ULTIMATE_PERFORMANCE = new("e9a42b02-d5df-448d-aa00-03f14749eb61");
    public static readonly Guid GUID_POWER_SAVER = new("a1841308-3541-4fab-bc81-f71556f20b4a");

    // ==========================================
    // SYSTEM RESTORE POINT API (srclient.dll)
    // ==========================================
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RESTOREPOINTINFO
    {
        public int dwEventType;
        public int dwRestorePtType;
        public long llSequenceNumber;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szDescription;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STATEMGRSTATUS
    {
        public int nStatus;
        public long llSequenceNumber;
    }

    [DllImport("srclient.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool SRSetRestorePointW(ref RESTOREPOINTINFO pRestorePointSpec, out STATEMGRSTATUS pSMgrStatus);

    public const int BEGIN_SYSTEM_CHANGE = 100;
    public const int END_SYSTEM_CHANGE = 101;
    public const int MODIFY_SETTINGS = 12;

    // ==========================================
    // SHELL & RECYCLE BIN (Shell32.dll)
    // ==========================================
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SHQUERYRBINFO
    {
        public uint cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern int SHQueryRecycleBin(string? pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    public static extern uint SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    public const uint SHERB_NOCONFIRMATION = 0x00000001;
    public const uint SHERB_NOPROGRESSUI = 0x00000002;
    public const uint SHERB_NOSOUND = 0x00000004;

    // ==========================================
    // KERNEL SYSTEM TIMES (Kernel32.dll)
    // ==========================================
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetSystemTimes(
        out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime,
        out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime,
        out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime
    );
}
