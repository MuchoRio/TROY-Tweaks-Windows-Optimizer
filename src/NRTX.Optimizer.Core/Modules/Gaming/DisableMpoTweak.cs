using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class DisableMpoTweak : ITweak
{
    public string Id => "gaming.disable_mpo";
    public string Name => "Disable Multi-Plane Overlay (MPO) DWM Stutter & Flicker Fix";
    public string Description => "Disables Multi-Plane Overlay (OverlayTestMode 5) in Desktop Window Manager (DWM), fixing micro-stutters, black screens, and display driver timeouts on NVIDIA & AMD GPUs.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => true;

    private const string DwmKey = @"SOFTWARE\Microsoft\Windows\Dwm";

    public Task<bool> IsAppliedAsync()
    {
        var val = SafeRegistry.GetDword(RegistryHive.LocalMachine, DwmKey, "OverlayTestMode");
        return Task.FromResult(val == 5);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Multi-Plane Overlay (MPO) would be disabled via OverlayTestMode=5.", isDryRun: true));

        RegistryBackupEngine.BackupKey(@"HKEY_LOCAL_MACHINE\" + DwmKey, "dwm_mpo");
        bool ok = SafeRegistry.SetDword(RegistryHive.LocalMachine, DwmKey, "OverlayTestMode", 5);

        return Task.FromResult(ok
            ? ExecutionResult.Ok("Multi-Plane Overlay (MPO) disabled. Restart required for display driver.")
            : ExecutionResult.Fail("Failed to update DWM OverlayTestMode registry key."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Multi-Plane Overlay (MPO) would be restored.", isDryRun: true));

        bool ok = SafeRegistry.DeleteValue(RegistryHive.LocalMachine, DwmKey, "OverlayTestMode");

        return Task.FromResult(ok
            ? ExecutionResult.Ok("Multi-Plane Overlay (MPO) restored to Windows default.")
            : ExecutionResult.Fail("Failed to remove OverlayTestMode registry key."));
    }
}
