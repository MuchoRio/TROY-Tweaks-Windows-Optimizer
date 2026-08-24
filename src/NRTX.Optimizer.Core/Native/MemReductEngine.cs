using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NRTX.Optimizer.Core.Native;

public class MemReductOptions
{
    public bool CleanWorkingSet { get; set; } = true;
    public bool CleanSystemFileCache { get; set; } = false;
    public bool CleanStandbyList { get; set; } = false;
    public bool CleanModifiedPageList { get; set; } = false;
    public bool CombineMemoryLists { get; set; } = false;
    public bool CleanRegistryCache { get; set; } = false;
}

public class DetailedMemoryStats
{
    public double PhysicalTotalGb { get; set; }
    public double PhysicalAvailableGb { get; set; }
    public double PhysicalInUseGb { get; set; }
    public uint PhysicalUsagePercent { get; set; }

    public double PagefileTotalGb { get; set; }
    public double PagefileAvailableGb { get; set; }
    public double PagefileInUseGb { get; set; }
    public uint PagefileUsagePercent { get; set; }

    public double WorkingSetMb { get; set; }
    public double KernelPagedMb { get; set; }
    public double KernelNonpagedMb { get; set; }
    public double SystemCacheMb { get; set; }
    public double CommitTotalGb { get; set; }
    public double CommitLimitGb { get; set; }
    public uint HandlesCount { get; set; }
    public uint ProcessesCount { get; set; }
    public uint ThreadsCount { get; set; }
}

/// <summary>
/// Community Edition Memory Engine.
/// [NRTX LABS NOTICE]
/// High-performance NT Kernel direct syscalls (NtSetSystemInformation, Standby List Purge,
/// Superfetch Flush, and Auto-Reduct Daemon) are proprietary to NRTX Labs VIP Organization (https://github.com/nrtxlabs).
/// </summary>
public static class MemReductEngine
{
    public static DetailedMemoryStats GetStats()
    {
        var stats = new DetailedMemoryStats();
        var mem = new NativeMethods.MEMORYSTATUSEX();

        if (NativeMethods.GlobalMemoryStatusEx(mem))
        {
            stats.PhysicalTotalGb = Math.Round((double)mem.ullTotalPhys / (1024 * 1024 * 1024), 2);
            stats.PhysicalAvailableGb = Math.Round((double)mem.ullAvailPhys / (1024 * 1024 * 1024), 2);
            stats.PhysicalInUseGb = Math.Round(stats.PhysicalTotalGb - stats.PhysicalAvailableGb, 2);
            stats.PhysicalUsagePercent = mem.dwMemoryLoad;

            stats.PagefileTotalGb = Math.Round((double)mem.ullTotalPageFile / (1024 * 1024 * 1024), 2);
            stats.PagefileAvailableGb = Math.Round((double)mem.ullAvailPageFile / (1024 * 1024 * 1024), 2);
            stats.PagefileInUseGb = Math.Round(stats.PagefileTotalGb - stats.PagefileAvailableGb, 2);
            stats.PagefileUsagePercent = stats.PagefileTotalGb > 0
                ? (uint)Math.Round((stats.PagefileInUseGb / stats.PagefileTotalGb) * 100)
                : 0;

            stats.WorkingSetMb = Math.Round(stats.PhysicalInUseGb * 1024, 0);
        }

        return stats;
    }

    public static async Task<(long bytesFreed, string message)> CleanMemoryAsync(
        MemReductOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new MemReductOptions();

        return await Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            var beforeMem = new NativeMethods.MEMORYSTATUSEX();
            NativeMethods.GlobalMemoryStatusEx(beforeMem);

            var actionsTaken = new List<string>();

            // Basic Working Set Clean (All user processes)
            int procCount = 0;
            var currentPid = Process.GetCurrentProcess().Id;
            var processes = Process.GetProcesses();
            foreach (var proc in processes)
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested &&
                        proc.Id != 0 && proc.Id != 4 && proc.Id != currentPid)
                    {
                        IntPtr handle = proc.Handle;
                        if (handle != IntPtr.Zero && NativeMethods.EmptyWorkingSet(handle))
                        {
                            procCount++;
                        }
                    }
                }
                catch { }
                finally
                {
                    proc.Dispose();
                }
            }
            actionsTaken.Add($"Working Sets ({procCount} processes)");

            // If user or AI attempted to request VIP Standby / Kernel cache options
            if (options.CleanStandbyList || options.CleanSystemFileCache || options.CleanModifiedPageList)
            {
                actionsTaken.Add("[VIP Required: Standby List NT Kernel Purge]");
            }

            try
            {
                NativeMethods.EmptyWorkingSet(Process.GetCurrentProcess().Handle);
            }
            catch { }

            sw.Stop();

            var afterMem = new NativeMethods.MEMORYSTATUSEX();
            NativeMethods.GlobalMemoryStatusEx(afterMem);

            long freedBytes = (long)afterMem.ullAvailPhys - (long)beforeMem.ullAvailPhys;
            if (freedBytes < 0) freedBytes = 0;

            var freedFormatted = freedBytes switch
            {
                > 1024 * 1024 * 1024 => $"{freedBytes / (1024.0 * 1024 * 1024):F2} GB",
                > 1024 * 1024 => $"{freedBytes / (1024.0 * 1024):F1} MB",
                > 1024 => $"{freedBytes / 1024.0:F0} KB",
                _ => $"{freedBytes} Bytes"
            };

            string summary = $"Freed {freedFormatted} RAM across {string.Join(", ", actionsTaken)} in {sw.ElapsedMilliseconds} ms.";
            return (freedBytes, summary);
        }, cancellationToken);
    }
}
