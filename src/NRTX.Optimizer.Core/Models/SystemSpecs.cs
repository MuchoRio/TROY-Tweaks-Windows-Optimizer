using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Native;

namespace NRTX.Optimizer.Core.Models;

public enum CpuVendorType
{
    Intel,
    Amd,
    Other
}

public enum GpuVendorType
{
    NvidiaRtx,
    NvidiaGtx,
    AmdRadeon,
    IntelArc,
    Generic
}

public class SystemSpecs
{
    public string OsName { get; set; } = string.Empty;
    public string OsBuild { get; set; } = string.Empty;
    public string CpuName { get; set; } = string.Empty;
    public string GpuName { get; set; } = string.Empty;
    public double TotalRamGb { get; set; }
    public double AvailableRamGb { get; set; }
    public uint MemoryLoadPercent { get; set; }
    public string ActivePowerPlan { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public DisplayInfo Display { get; set; } = new();

    public CpuVendorType CpuVendor => DetectCpuVendor(CpuName);
    public GpuVendorType GpuVendor => DetectGpuVendor(GpuName);

    public string CpuArchitectureBadge => CpuVendor switch
    {
        CpuVendorType.Intel when CpuName.Contains("12th", StringComparison.OrdinalIgnoreCase) || 
                                 CpuName.Contains("13th", StringComparison.OrdinalIgnoreCase) || 
                                 CpuName.Contains("14th", StringComparison.OrdinalIgnoreCase) || 
                                 CpuName.Contains("Ultra", StringComparison.OrdinalIgnoreCase) => "🔵 Intel Core (P/E-Core Hybrid)",
        CpuVendorType.Intel => "🔵 Intel Core Architecture",
        CpuVendorType.Amd when CpuName.Contains("X3D", StringComparison.OrdinalIgnoreCase) => "🔴 AMD Ryzen (3D V-Cache)",
        CpuVendorType.Amd => "🔴 AMD Ryzen Architecture",
        _ => "⚙️ Standard CPU"
    };

    public string GpuArchitectureBadge => GpuVendor switch
    {
        GpuVendorType.NvidiaRtx => "🟢 NVIDIA GeForce RTX (Reflex / FrameGen)",
        GpuVendorType.NvidiaGtx => "🟢 NVIDIA GeForce GTX (Low Latency D3D)",
        GpuVendorType.AmdRadeon => "🔴 AMD Radeon (RDNA Anti-Lag)",
        GpuVendorType.IntelArc => "🔵 Intel Arc Graphics (XeSS Ready)",
        _ => "🎮 Standard Graphics"
    };

    private static CpuVendorType DetectCpuVendor(string cpuName)
    {
        if (cpuName.Contains("Intel", StringComparison.OrdinalIgnoreCase)) return CpuVendorType.Intel;
        if (cpuName.Contains("AMD", StringComparison.OrdinalIgnoreCase) || cpuName.Contains("Ryzen", StringComparison.OrdinalIgnoreCase)) return CpuVendorType.Amd;
        return CpuVendorType.Other;
    }

    private static GpuVendorType DetectGpuVendor(string gpuName)
    {
        if (gpuName.Contains("RTX", StringComparison.OrdinalIgnoreCase)) return GpuVendorType.NvidiaRtx;
        if (gpuName.Contains("GTX", StringComparison.OrdinalIgnoreCase) || gpuName.Contains("GeForce", StringComparison.OrdinalIgnoreCase)) return GpuVendorType.NvidiaGtx;
        if (gpuName.Contains("Radeon", StringComparison.OrdinalIgnoreCase) || gpuName.Contains("AMD", StringComparison.OrdinalIgnoreCase)) return GpuVendorType.AmdRadeon;
        if (gpuName.Contains("Arc", StringComparison.OrdinalIgnoreCase)) return GpuVendorType.IntelArc;
        return GpuVendorType.Generic;
    }

    public static async Task<SystemSpecs> CollectAsync()
    {
        string osBuildStr = Environment.OSVersion.Version.ToString();
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var bNum = key?.GetValue("CurrentBuildNumber")?.ToString();
            var ubr = key?.GetValue("UBR")?.ToString();
            if (!string.IsNullOrEmpty(bNum))
            {
                osBuildStr = !string.IsNullOrEmpty(ubr) ? $"{bNum}.{ubr}" : bNum;
            }
        }
        catch { }

        var specs = new SystemSpecs
        {
            OsName = GetOsDescription(),
            OsBuild = osBuildStr,
            IsAdmin = Safety.PrivilegeGuard.IsAdministrator()
        };

        // RAM via GlobalMemoryStatusEx
        var mem = new NativeMethods.MEMORYSTATUSEX();
        if (NativeMethods.GlobalMemoryStatusEx(mem))
        {
            specs.TotalRamGb = Math.Round((double)mem.ullTotalPhys / (1024 * 1024 * 1024), 1);
            specs.AvailableRamGb = Math.Round((double)mem.ullAvailPhys / (1024 * 1024 * 1024), 1);
            specs.MemoryLoadPercent = mem.dwMemoryLoad;
        }

        // Concurrent Subsystems Query: Display, CPU, GPU, and Power Plan
        var displayTask = Task.Run(() =>
        {
            try
            {
                return DisplayInfoService.GetPrimaryDisplayInfo();
            }
            catch
            {
                return new DisplayInfo();
            }
        });

        var cpuTask = Task.Run(() =>
        {
            try
            {
                using var cpuKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                var regCpu = cpuKey?.GetValue("ProcessorNameString")?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(regCpu))
                {
                    return regCpu;
                }

                using var cpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                foreach (var obj in cpuSearcher.Get())
                {
                    var name = obj["Name"]?.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
            }
            catch { }

            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Intel / AMD Processor";
        });

        var gpuTask = Task.Run(() =>
        {
            try
            {
                string? detectedGpu = null;
                using var classKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
                if (classKey != null)
                {
                    foreach (var sub in classKey.GetSubKeyNames())
                    {
                        if (sub.StartsWith("000", StringComparison.OrdinalIgnoreCase))
                        {
                            using var subKey = classKey.OpenSubKey(sub);
                            var desc = subKey?.GetValue("DriverDesc")?.ToString();
                            if (!string.IsNullOrWhiteSpace(desc) && !desc.Contains("Basic Display", StringComparison.OrdinalIgnoreCase))
                            {
                                detectedGpu = desc;
                                break;
                            }
                            else if (!string.IsNullOrWhiteSpace(desc) && detectedGpu == null)
                            {
                                detectedGpu = desc;
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(detectedGpu))
                {
                    return detectedGpu;
                }

                using var gpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                foreach (var obj in gpuSearcher.Get())
                {
                    var name = obj["Name"]?.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
            }
            catch { }

            return "Graphics Adapter";
        });

        var powerTask = Task.Run(() =>
        {
            try
            {
                if (NativeMethods.PowerGetActiveScheme(IntPtr.Zero, out var guidPtr) == 0 && guidPtr != IntPtr.Zero)
                {
                    var activeGuid = Marshal.PtrToStructure<Guid>(guidPtr);
                    NativeMethods.LocalFree(guidPtr);

                    if (activeGuid == NativeMethods.GUID_ULTIMATE_PERFORMANCE)
                        return "Ultimate Performance";
                    if (activeGuid == NativeMethods.GUID_HIGH_PERFORMANCE)
                        return "High Performance";
                    if (activeGuid == NativeMethods.GUID_BALANCED)
                        return "Balanced";
                    if (activeGuid == NativeMethods.GUID_POWER_SAVER)
                        return "Power Saver";

                    return $"Custom ({activeGuid.ToString().Substring(0, 8)})";
                }
            }
            catch { }

            return "Balanced (Default)";
        });

        await Task.WhenAll(displayTask, cpuTask, gpuTask, powerTask);

        specs.Display = await displayTask;
        specs.CpuName = await cpuTask;
        specs.GpuName = await gpuTask;
        specs.ActivePowerPlan = await powerTask;

        return specs;
    }

    private static string GetOsDescription()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var productName = key?.GetValue("ProductName")?.ToString() ?? "Windows";
            var displayVersion = key?.GetValue("DisplayVersion")?.ToString() ?? key?.GetValue("ReleaseId")?.ToString();
            var currentBuildStr = key?.GetValue("CurrentBuildNumber")?.ToString() ?? key?.GetValue("CurrentBuild")?.ToString();

            int.TryParse(currentBuildStr, out int buildNumber);
            if (buildNumber == 0)
            {
                buildNumber = Environment.OSVersion.Version.Build;
            }

            // Microsoft maintains ProductName as "Windows 10" in the registry for legacy application compatibility.
            // All NT builds >= 22000 are Windows 11.
            if (buildNumber >= 22000 && productName.Contains("Windows 10", StringComparison.OrdinalIgnoreCase))
            {
                productName = productName.Replace("Windows 10", "Windows 11", StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(displayVersion))
            {
                return $"{productName} {displayVersion}".Trim();
            }

            return $"{productName}".Trim();
        }
        catch { }

        var fallback = RuntimeInformation.OSDescription;
        if (Environment.OSVersion.Version.Build >= 22000 && fallback.Contains("Windows 10", StringComparison.OrdinalIgnoreCase))
        {
            fallback = fallback.Replace("Windows 10", "Windows 11", StringComparison.OrdinalIgnoreCase);
        }
        return fallback;
    }
}
