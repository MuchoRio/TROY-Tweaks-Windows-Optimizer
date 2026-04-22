using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class MouseRawInputTweak : ITweak
{
    public string Id => "gaming.mouse_raw_input_1to1";
    public string Name => "1:1 Raw Mouse Sensor Input & Disable Windows Acceleration";
    public string Description => "Enforces 1:1 hardware pixel mapping by setting MouseSpeed=0, MouseThreshold1=0, MouseThreshold2=0, and setting MouseSensitivity to 10 (6/11 default notch), eliminating Windows pointer curve acceleration for 100% muscle memory flick shot consistency.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string MouseKey = @"Control Panel\Mouse";

    public Task<bool> IsAppliedAsync()
    {
        var mouseSpeed = SafeRegistry.GetString(RegistryHive.CurrentUser, MouseKey, "MouseSpeed");
        var thresh1 = SafeRegistry.GetString(RegistryHive.CurrentUser, MouseKey, "MouseThreshold1");
        var thresh2 = SafeRegistry.GetString(RegistryHive.CurrentUser, MouseKey, "MouseThreshold2");
        var sensitivity = SafeRegistry.GetString(RegistryHive.CurrentUser, MouseKey, "MouseSensitivity");

        bool applied = mouseSpeed == "0" && thresh1 == "0" && thresh2 == "0" && sensitivity == "10";
        return Task.FromResult(applied);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Windows mouse acceleration would be disabled and sensitivity set to 1:1 notch.", isDryRun: true));

        SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKey, "MouseSpeed", "0");
        SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKey, "MouseThreshold1", "0");
        SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKey, "MouseThreshold2", "0");
        SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKey, "MouseSensitivity", "10");

        return Task.FromResult(ExecutionResult.Ok("1:1 Raw Mouse Sensor tracking applied (Acceleration: OFF, Sensitivity: 6/11 notch)."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Windows default mouse settings would be restored.", isDryRun: true));

        SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKey, "MouseSpeed", "1");
        SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKey, "MouseThreshold1", "6");
        SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKey, "MouseThreshold2", "10");
        SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKey, "MouseSensitivity", "10");

        return Task.FromResult(ExecutionResult.Ok("Default Windows mouse settings restored."));
    }
}
