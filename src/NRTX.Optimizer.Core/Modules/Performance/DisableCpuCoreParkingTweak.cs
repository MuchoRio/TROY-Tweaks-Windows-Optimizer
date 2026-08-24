using System.Diagnostics;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Performance;

/// <summary>
/// Disables CPU Core Parking on Intel Core & AMD Ryzen processors.
/// Unparks 100% of logical processor cores so they never transition to idle sleep states during gaming.
/// </summary>
public class DisableCpuCoreParkingTweak : ITweak
{
    public string Id => "perf.cpu_core_parking_disable";
    public string Name => "Disable CPU Core Parking (Unpark All Intel & AMD Ryzen Logical Cores)";
    public string Description => "Forces Windows power manager to keep 100% of CPU cores unparked and awake, preventing sudden thread wake-up micro-stuttering.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string CoreParkingSettingGuid = "0cc5b647-c1df-4637-891a-dec35c318583";
    private const string ProcessorSubgroupGuid = "54533251-82be-4824-96c1-47b60b740d00";
    private const string PowerSettingsPath = @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\" + ProcessorSubgroupGuid + @"\" + CoreParkingSettingGuid;

    public Task<bool> IsAppliedAsync()
    {
        var valMin = SafeRegistry.GetDword(RegistryHive.LocalMachine, PowerSettingsPath, "ValueMin");
        return Task.FromResult(valMin == 100);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: All Intel & AMD CPU cores would be unparked (ValueMin=100%, ValueMax=100%).", isDryRun: true));

        try
        {
            // Unhide the core parking setting in powercfg
            SafeRegistry.SetDword(RegistryHive.LocalMachine, PowerSettingsPath, "Attributes", 0);
            SafeRegistry.SetDword(RegistryHive.LocalMachine, PowerSettingsPath, "ValueMin", 100);
            SafeRegistry.SetDword(RegistryHive.LocalMachine, PowerSettingsPath, "ValueMax", 100);

            // Execute powercfg to apply across active scheme
            RunPowerCfg($"/setacvalueindex SCHEME_CURRENT {ProcessorSubgroupGuid} {CoreParkingSettingGuid} 100");
            RunPowerCfg($"/setdcvalueindex SCHEME_CURRENT {ProcessorSubgroupGuid} {CoreParkingSettingGuid} 100");
            RunPowerCfg("/setactive SCHEME_CURRENT");

            AuditLogger.Log(AuditLogLevel.Success, "CoreParking", "CPU Core Parking disabled; 100% of logical cores unparked.");
            return Task.FromResult(ExecutionResult.Ok("CPU Core Parking disabled. All CPU cores are 100% unparked."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "CoreParking", $"Failed to disable CPU core parking: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: CPU Core parking would be restored to Windows default scheme.", isDryRun: true));

        try
        {
            SafeRegistry.SetDword(RegistryHive.LocalMachine, PowerSettingsPath, "Attributes", 1);
            SafeRegistry.SetDword(RegistryHive.LocalMachine, PowerSettingsPath, "ValueMin", 0);
            SafeRegistry.SetDword(RegistryHive.LocalMachine, PowerSettingsPath, "ValueMax", 100);

            RunPowerCfg($"/setacvalueindex SCHEME_CURRENT {ProcessorSubgroupGuid} {CoreParkingSettingGuid} 0");
            RunPowerCfg("/setactive SCHEME_CURRENT");

            AuditLogger.Log(AuditLogLevel.Info, "CoreParking", "CPU Core Parking restored to Windows default policy.");
            return Task.FromResult(ExecutionResult.Ok("CPU Core Parking restored to Windows default."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "CoreParking", $"Failed to rollback core parking: {ex.Message}");
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
