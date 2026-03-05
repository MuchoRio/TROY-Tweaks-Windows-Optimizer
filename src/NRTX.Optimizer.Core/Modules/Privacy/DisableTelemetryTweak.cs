using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Privacy;

public class DisableTelemetryTweak : ITweak
{
    public string Id => "privacy.disable_telemetry";
    public string Name => "Disable Windows Telemetry & Data Collection";
    public string Description => "Sets Windows Diagnostic data collection to Security/Disabled (0), preventing background telemetry transmissions.";
    public TweakCategory Category => TweakCategory.Privacy;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => true;

    private const string TelemetryKey = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection";

    public Task<bool> IsAppliedAsync()
    {
        var val = SafeRegistry.GetDword(RegistryHive.LocalMachine, TelemetryKey, "AllowTelemetry");
        return Task.FromResult(val == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Telemetry would be set to 0 (Security).", isDryRun: true));

        RegistryBackupEngine.BackupKey(@"HKEY_LOCAL_MACHINE\" + TelemetryKey, "telemetry");
        
        bool ok1 = SafeRegistry.SetDword(RegistryHive.LocalMachine, TelemetryKey, "AllowTelemetry", 0);
        bool ok2 = SafeRegistry.SetDword(RegistryHive.LocalMachine, TelemetryKey, "MaxTelemetryAllowed", 0);

        return Task.FromResult(ok1 && ok2
            ? ExecutionResult.Ok("Windows Telemetry has been disabled successfully.")
            : ExecutionResult.Fail("Failed to apply Telemetry registry key. Check Administrator permissions."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Telemetry would be restored to Windows Default.", isDryRun: true));

        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, TelemetryKey, "AllowTelemetry");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, TelemetryKey, "MaxTelemetryAllowed");

        return Task.FromResult(ExecutionResult.Ok("Windows Telemetry restored to default state."));
    }
}
