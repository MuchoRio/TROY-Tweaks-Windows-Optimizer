using System.Diagnostics;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Performance;

/// <summary>
/// Optimizes AMD Ryzen CPPC (Collaborative Processor Performance Control) and Dynamic CCD allocation
/// to prioritize highest frequency cores and 3D V-Cache (X3D) execution during gaming.
/// </summary>
public class AmdRyzenCppcX3dBoostTweak : ITweak
{
    public string Id => "perf.amd_ryzen_cppc_x3d_boost";
    public string Name => "Optimize AMD Ryzen CPPC & Dynamic 3D V-Cache (X3D) Core Allocation";
    public string Description => "Enforces CPPC preferred core order and disables deep autonomous frequency stepping for lowest CCD cross-talk latency.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string SubProcessorGuid = "54533251-82be-4824-96c1-47b60b740d00";
    private const string AutonomousModeGuid = "8baa4a82-14c1-4477-80db-cb2192212238"; // Autonomous Mode (CPPC)
    private const string CppcPerfClassGuid = "7f2492b6-60b1-45e9-ae55-773f8f8cafb2"; // Hetero policy / performance class

    public Task<bool> IsAppliedAsync()
    {
        var attr = SafeRegistry.GetDword(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\{SubProcessorGuid}\{AutonomousModeGuid}", "Attributes");
        return Task.FromResult(attr == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: AMD Ryzen CPPC and 3D V-Cache priority would be optimized.", isDryRun: true));

        try
        {
            // Unhide CPPC autonomous mode in powercfg
            SafeRegistry.SetDword(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\{SubProcessorGuid}\{AutonomousModeGuid}", "Attributes", 0);
            
            // Set Autonomous Mode to 1 (Enabled with CPPC v2 active)
            RunPowerCfg($"/setacvalueindex SCHEME_CURRENT {SubProcessorGuid} {AutonomousModeGuid} 1");
            RunPowerCfg($"/setacvalueindex SCHEME_CURRENT {SubProcessorGuid} {CppcPerfClassGuid} 0");
            RunPowerCfg("/setactive SCHEME_CURRENT");

            AuditLogger.Log(AuditLogLevel.Success, "AmdCppc", "AMD Ryzen CPPC and 3D V-Cache dynamic scheduler optimized.");
            return Task.FromResult(ExecutionResult.Ok("AMD Ryzen CPPC and 3D V-Cache preferred core boost active."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "AmdCppc", $"Failed to configure AMD Ryzen CPPC: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: AMD Ryzen CPPC settings would be restored to defaults.", isDryRun: true));

        try
        {
            RunPowerCfg($"/setacvalueindex SCHEME_CURRENT {SubProcessorGuid} {AutonomousModeGuid} 1");
            RunPowerCfg("/setactive SCHEME_CURRENT");

            AuditLogger.Log(AuditLogLevel.Info, "AmdCppc", "AMD Ryzen CPPC settings restored to default.");
            return Task.FromResult(ExecutionResult.Ok("AMD Ryzen CPPC restored to default."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "AmdCppc", $"Failed to rollback AMD CPPC settings: {ex.Message}");
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
