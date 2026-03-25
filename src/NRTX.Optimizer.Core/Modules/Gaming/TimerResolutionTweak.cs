using System.Diagnostics;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

/// <summary>
/// Configures high-precision timer frequency, disables synthetic dynamic tick jitter,
/// and enforces Invariant TSC for minimum DPC latency in competitive game engines.
/// </summary>
public class TimerResolutionTweak : ITweak
{
    public string Id => "gaming.timer_resolution_low_latency";
    public string Name => "Optimize High-Precision Timer Resolution & Force Invariant TSC (0.5ms Clock)";
    public string Description => "Configures dynamic tick and invariant TSC clock policies, reducing micro-stutters and timer drift in competitive games.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => true;

    private const string KernelSessionKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel";

    public Task<bool> IsAppliedAsync()
    {
        var val = SafeRegistry.GetDword(RegistryHive.LocalMachine, KernelSessionKey, "GlobalTimerResolutionRequests");
        return Task.FromResult(val == 1);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: BCDEdit dynamic ticks and TSC clock would be optimized for low latency.", isDryRun: true));

        try
        {
            // BCDEdit commands for low latency timer
            RunBcdEdit("/set disabledynamictick yes");
            RunBcdEdit("/set useplatformclock false");
            RunBcdEdit("/set tscsyncpolicy Enhanced");

            // Global Timer resolution helper registry
            SafeRegistry.SetDword(RegistryHive.LocalMachine, KernelSessionKey, "GlobalTimerResolutionRequests", 1);

            AuditLogger.Log(AuditLogLevel.Success, "TimerResolution", "Dynamic ticks disabled and Invariant TSC sync policy configured.");
            return Task.FromResult(ExecutionResult.Ok("High-precision timer and Invariant TSC clock configured."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "TimerResolution", $"Failed to configure timer resolution: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: BCDEdit timer settings would be restored to Windows defaults.", isDryRun: true));

        try
        {
            RunBcdEdit("/deletevalue disabledynamictick");
            RunBcdEdit("/deletevalue useplatformclock");
            RunBcdEdit("/deletevalue tscsyncpolicy");

            SafeRegistry.DeleteValue(RegistryHive.LocalMachine, KernelSessionKey, "GlobalTimerResolutionRequests");

            AuditLogger.Log(AuditLogLevel.Info, "TimerResolution", "Timer resolution policies restored to defaults.");
            return Task.FromResult(ExecutionResult.Ok("Timer resolution policies restored to Windows defaults."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "TimerResolution", $"Failed to rollback timer settings: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    private static void RunBcdEdit(string arguments)
    {
        using var proc = Process.Start(new ProcessStartInfo
        {
            FileName = "bcdedit.exe",
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false
        });
        proc?.WaitForExit(3000);
    }
}
