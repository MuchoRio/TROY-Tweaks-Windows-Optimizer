using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class DisableFullscreenOptimizationsTweak : ITweak
{
    public string Id => "gaming.fse_behavior";
    public string Name => "Optimize Fullscreen Window Display Layer (Low-Latency DWM)";
    public string Description => "Configures Desktop Window Manager (DWM) for direct swapchain presentation and reduced composition delay.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string GameConfigKey = @"System\GameConfigStore";

    public Task<bool> IsAppliedAsync()
    {
        var fse = SafeRegistry.GetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_DXGIHonorFSEWindowsCompatible");
        return Task.FromResult(fse == 1);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Fullscreen display pipeline would be optimized.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_DXGIHonorFSEWindowsCompatible", 1);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_FSEBehavior", 2);

        return Task.FromResult(ExecutionResult.Ok("Direct display swapchain layer optimized."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Fullscreen display settings would be reverted.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_DXGIHonorFSEWindowsCompatible", 0);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, GameConfigKey, "GameDVR_FSEBehavior", 0);

        return Task.FromResult(ExecutionResult.Ok("Fullscreen display settings reverted to default."));
    }
}
