using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class DisableGameDvrTweak : ITweak
{
    public string Id => "gaming.disable_game_dvr";
    public string Name => "Disable Xbox GameDVR Background Capture & Overlay Latency";
    public string Description => "Disables Xbox Game Bar background recording, reducing micro-stuttering and input lag in games.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    private const string GameConfigKey = @"System\GameConfigStore";
    private const string PolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\GameDVR";

    public Task<bool> IsAppliedAsync()
    {
        var dvr = SafeRegistry.GetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_Enabled");
        var pol = SafeRegistry.GetDword(RegistryHive.LocalMachine, PolicyKey, "AllowGameDVR");
        return Task.FromResult(dvr == 0 && pol == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Xbox GameDVR would be disabled.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_Enabled", 0);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_FSEBehaviorMode", 2);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_HonorUserFSEBehaviorMode", 1);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_DXGIHonorFSEWindowsCompatible", 1);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, PolicyKey, "AllowGameDVR", 0);

        return Task.FromResult(ExecutionResult.Ok("Xbox GameDVR & Background Recording disabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Xbox GameDVR would be restored.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_Enabled", 1);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_FSEBehaviorMode", 0);
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, PolicyKey, "AllowGameDVR");

        return Task.FromResult(ExecutionResult.Ok("Xbox GameDVR settings restored."));
    }
}
