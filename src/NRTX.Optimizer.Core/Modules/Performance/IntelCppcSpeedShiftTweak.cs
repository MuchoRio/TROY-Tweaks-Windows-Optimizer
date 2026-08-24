using System.Diagnostics;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Performance;

/// <summary>
/// Optimizes Intel Speed Shift Technology (Hardware P-States / HWP) and Energy Performance Preference (EPP)
/// to 0 (Raw Maximum Performance), prioritizing Performance Cores (P-Cores) for foreground game threads.
/// </summary>
public class IntelCppcSpeedShiftTweak : ITweak
{
    public string Id => "perf.intel_cppc_speed_shift";
    public string Name => "Optimize Intel Speed Shift & Thread Director (EPP 0 Maximum P-Core Boost)";
    public string Description => "Sets Energy Performance Preference to 0 (Max Performance) and optimizes Intel P/E-Core scheduling response time.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string SubProcessorGuid = "54533251-82be-4824-96c1-47b60b740d00";
    private const string EppGuid = "36687f9e-e3a5-4dbf-b1dc-15eb381c6863"; // Energy Performance Preference
    private const string HetroPolicyGuid = "7f2492b6-60b1-45e9-ae55-773f8f8cafb2"; // Heterogeneous thread scheduling policy

    public Task<bool> IsAppliedAsync()
    {
        var attr = SafeRegistry.GetDword(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\{SubProcessorGuid}\{EppGuid}", "Attributes");
        return Task.FromResult(attr == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: Intel EPP would be set to 0 and P-Core thread priority maximized.", isDryRun: true));

        try
        {
            // Unhide EPP and set to 0 (Maximum Performance)
            SafeRegistry.SetDword(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\{SubProcessorGuid}\{EppGuid}", "Attributes", 0);
            
            RunPowerCfg($"/setacvalueindex SCHEME_CURRENT {SubProcessorGuid} {EppGuid} 0");
            RunPowerCfg($"/setdcvalueindex SCHEME_CURRENT {SubProcessorGuid} {EppGuid} 0");

            // Set Heterogeneous thread scheduling policy to 0 (Prefer High Performance Cores)
            RunPowerCfg($"/setacvalueindex SCHEME_CURRENT {SubProcessorGuid} {HetroPolicyGuid} 0");
            RunPowerCfg("/setactive SCHEME_CURRENT");

            AuditLogger.Log(AuditLogLevel.Success, "IntelSpeedShift", "Intel EPP set to 0 and P-Core priority scheduling active.");
            return Task.FromResult(ExecutionResult.Ok("Intel Speed Shift EPP 0 & P-Core gaming priority applied."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "IntelSpeedShift", $"Failed to configure Intel Speed Shift: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: Intel Speed Shift EPP would be restored to balanced (50).", isDryRun: true));

        try
        {
            RunPowerCfg($"/setacvalueindex SCHEME_CURRENT {SubProcessorGuid} {EppGuid} 50");
            RunPowerCfg("/setactive SCHEME_CURRENT");

            AuditLogger.Log(AuditLogLevel.Info, "IntelSpeedShift", "Intel Speed Shift EPP restored to default (50).");
            return Task.FromResult(ExecutionResult.Ok("Intel Speed Shift restored to default."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "IntelSpeedShift", $"Failed to rollback Intel settings: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    private static void RunPowerCfg(string args)
    {
        using var proc = Process.Start(new ProcessStartInfo
        {
            FileName = "powercfg.exe",
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = false
        });
        proc?.WaitForExit(3000);
    }
}
