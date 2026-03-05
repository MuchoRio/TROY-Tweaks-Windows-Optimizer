using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Privacy;

public class DisableLocationTrackingTweak : ITweak
{
    public string Id => "privacy.disable_location_tracking";
    public string Name => "Disable Windows Master Location Tracking Sensor";
    public string Description => "Disables the master location sensor service and prevents background geotracking.";
    public TweakCategory Category => TweakCategory.Privacy;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string SensorKey = @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors";
    private const string ConsentKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location";

    public Task<bool> IsAppliedAsync()
    {
        var val1 = SafeRegistry.GetDword(RegistryHive.LocalMachine, SensorKey, "DisableLocation");
        var val2 = SafeRegistry.GetString(RegistryHive.LocalMachine, ConsentKey, "Value");
        return Task.FromResult(val1 == 1 || val2 == "Deny");
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Master Location Tracking would be disabled.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, SensorKey, "DisableLocation", 1);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, SensorKey, "DisableLocationScripting", 1);
        SafeRegistry.SetString(RegistryHive.LocalMachine, ConsentKey, "Value", "Deny");

        return Task.FromResult(ExecutionResult.Ok("Location Tracking and Sensors disabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Location Tracking would be restored.", isDryRun: true));

        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, SensorKey, "DisableLocation");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, SensorKey, "DisableLocationScripting");
        SafeRegistry.SetString(RegistryHive.LocalMachine, ConsentKey, "Value", "Allow");

        return Task.FromResult(ExecutionResult.Ok("Location Tracking restored to default."));
    }
}
