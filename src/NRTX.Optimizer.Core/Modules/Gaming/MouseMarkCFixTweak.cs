using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class MouseMarkCFixTweak : ITweak
{
    public string Id => "gaming.mouse_markc_acceleration_fix";
    public string Name => "MarkC Windows 11 Mouse Acceleration Linear Curve Fix (100% Scaling)";
    public string Description => "Applies the legendary MarkC Windows Mouse Fix binary curves (SmoothMouseXCurve and SmoothMouseYCurve) calibrated for 100% desktop DPI scaling, eliminating legacy Windows kernel non-linear cursor acceleration.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string MouseKey = @"Control Panel\Mouse";

    // MarkC 100% 1:1 linear curve values
    private static readonly byte[] MarkCXCurve = new byte[]
    {
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x15, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x80, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00
    };

    private static readonly byte[] MarkCYCurve = new byte[]
    {
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x38, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0xa0, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0xe0, 0x00, 0x00, 0x00, 0x00, 0x00
    };

    public Task<bool> IsAppliedAsync()
    {
        var xCurve = SafeRegistry.GetBinary(RegistryHive.CurrentUser, MouseKey, "SmoothMouseXCurve");
        var yCurve = SafeRegistry.GetBinary(RegistryHive.CurrentUser, MouseKey, "SmoothMouseYCurve");

        bool applied = xCurve != null && yCurve != null &&
                       xCurve.Length == MarkCXCurve.Length &&
                       yCurve.Length == MarkCYCurve.Length &&
                       xCurve[8] == MarkCXCurve[8] &&
                       yCurve[10] == MarkCYCurve[10];

        return Task.FromResult(applied);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: MarkC 1:1 mouse acceleration curve fix would be applied.", isDryRun: true));

        SafeRegistry.SetBinary(RegistryHive.CurrentUser, MouseKey, "SmoothMouseXCurve", MarkCXCurve);
        SafeRegistry.SetBinary(RegistryHive.CurrentUser, MouseKey, "SmoothMouseYCurve", MarkCYCurve);

        return Task.FromResult(ExecutionResult.Ok("MarkC 100% 1:1 linear mouse acceleration curve applied successfully."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Default Windows smooth mouse curves would be restored.", isDryRun: true));

        // Default Windows curve
        byte[] defaultX = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xa0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x80, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        byte[] defaultY = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x66, 0xa6, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xcd, 0x4c, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xa0, 0x99, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x38, 0x33, 0x15, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        SafeRegistry.SetBinary(RegistryHive.CurrentUser, MouseKey, "SmoothMouseXCurve", defaultX);
        SafeRegistry.SetBinary(RegistryHive.CurrentUser, MouseKey, "SmoothMouseYCurve", defaultY);

        return Task.FromResult(ExecutionResult.Ok("Default Windows smooth mouse curves restored."));
    }
}
