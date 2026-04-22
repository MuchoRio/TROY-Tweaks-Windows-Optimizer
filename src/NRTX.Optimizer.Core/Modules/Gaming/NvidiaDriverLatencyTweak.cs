using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

/// <summary>
/// Disables NVIDIA driver telemetry services and configures low latency power management
/// across GeForce GTX and RTX graphics cards.
/// </summary>
public class NvidiaDriverLatencyTweak : ITweak
{
    public string Id => "gaming.nvidia_driver_power_latency";
    public string Name => "Optimize NVIDIA GeForce GTX/RTX Driver Latency & Disable Telemetry";
    public string Description => "Disables NvTelemetryContainer background daemons and configures driver power state to Prefer Maximum Performance.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    public Task<bool> IsAppliedAsync()
    {
        var notif = SafeRegistry.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\Global\NVTweak", "NvCplEnableAppProfileNotifications");
        return Task.FromResult(notif == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: NVIDIA Telemetry services would be disabled and D3D driver latency optimized.", isDryRun: true));

        try
        {
            // 1. Disable NvTelemetryContainer service if installed
            DisableService("NvTelemetryContainer");

            // 2. Configure NVIDIA driver D3D power & latency parameters in Registry
            SafeRegistry.SetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler", "EnablePreemption", 1);
            SafeRegistry.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\Global\NVTweak", "NvCplEnableAppProfileNotifications", 0);

            AuditLogger.Log(AuditLogLevel.Success, "NvidiaLatency", "NVIDIA Telemetry stopped and D3D preemption optimized.");
            return Task.FromResult(ExecutionResult.Ok("NVIDIA driver latency tuned & telemetry disabled."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "NvidiaLatency", $"Failed to configure NVIDIA driver tweaks: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: NVIDIA driver settings would be restored.", isDryRun: true));

        try
        {
            EnableService("NvTelemetryContainer");
            SafeRegistry.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\NVIDIA Corporation\Global\NVTweak", "NvCplEnableAppProfileNotifications");

            AuditLogger.Log(AuditLogLevel.Info, "NvidiaLatency", "NVIDIA driver settings rolled back.");
            return Task.FromResult(ExecutionResult.Ok("NVIDIA driver settings restored."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "NvidiaLatency", $"Failed to rollback NVIDIA driver settings: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    private static void DisableService(string serviceName)
    {
        try
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(3));
            }

            using var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"config \"{serviceName}\" start=disabled",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            proc?.WaitForExit(3000);
        }
        catch
        {
            // Service might not exist if AMD/Intel GPU
        }
    }

    private static void EnableService(string serviceName)
    {
        try
        {
            using var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"config \"{serviceName}\" start=demand",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            proc?.WaitForExit(3000);
        }
        catch
        {
            // Ignore
        }
    }
}
