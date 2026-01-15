using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

/// <summary>
/// Disables Windows built-in mouse acceleration and Enhance Pointer Precision.
/// Enforces true 1:1 hardware raw sensor polling for competitive FPS games (Valorant, Apex Legends, CS2).
/// </summary>
public class RawMouseInputTweak : ITweak
{
    public string Id => "gaming.raw_mouse_input";
    public string Name => "Disable Windows Mouse Acceleration & Enhance Pointer Precision (1:1 Raw Input)";
    public string Description => "Disables Windows cursor acceleration curve and sets mouse speed to true 1:1 linear mapping for pixel-perfect FPS aim.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string MouseKeyPath = @"Control Panel\Mouse";

    private readonly byte[] SmoothLinearCurve =
    [
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x15, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x90, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    ];

    private static readonly byte[] DefaultSmoothMouseXCurve =
    [
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0xa0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x40, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x80, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00
    ];

    private static readonly byte[] DefaultSmoothMouseYCurve =
    [
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x66, 0xa6, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xcd, 0x4c, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xa0, 0x99, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x38, 0x33, 0x15, 0x00, 0x00, 0x00, 0x00, 0x00
    ];

    public Task<bool> IsAppliedAsync()
    {
        var speed = SafeRegistry.GetString(RegistryHive.CurrentUser, MouseKeyPath, "MouseSpeed");
        var t1 = SafeRegistry.GetString(RegistryHive.CurrentUser, MouseKeyPath, "MouseThreshold1");
        var t2 = SafeRegistry.GetString(RegistryHive.CurrentUser, MouseKeyPath, "MouseThreshold2");

        return Task.FromResult(speed == "0" && t1 == "0" && t2 == "0");
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: Mouse acceleration would be disabled and set to 1:1 raw input.", isDryRun: true));

        try
        {
            SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKeyPath, "MouseSpeed", "0");
            SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKeyPath, "MouseThreshold1", "0");
            SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKeyPath, "MouseThreshold2", "0");
            SafeRegistry.SetBinary(RegistryHive.CurrentUser, MouseKeyPath, "SmoothMouseXCurve", SmoothLinearCurve);
            SafeRegistry.SetBinary(RegistryHive.CurrentUser, MouseKeyPath, "SmoothMouseYCurve", SmoothLinearCurve);

            AuditLogger.Log(AuditLogLevel.Success, "RawMouseInput", "Windows mouse acceleration disabled; 1:1 raw input curve enforced.");
            return Task.FromResult(ExecutionResult.Ok("Windows mouse acceleration disabled; 1:1 linear input active."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "RawMouseInput", $"Failed to configure raw mouse input: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: Mouse acceleration would be restored to Windows default.", isDryRun: true));

        try
        {
            SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKeyPath, "MouseSpeed", "1");
            SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKeyPath, "MouseThreshold1", "6");
            SafeRegistry.SetString(RegistryHive.CurrentUser, MouseKeyPath, "MouseThreshold2", "10");
            SafeRegistry.SetBinary(RegistryHive.CurrentUser, MouseKeyPath, "SmoothMouseXCurve", DefaultSmoothMouseXCurve);
            SafeRegistry.SetBinary(RegistryHive.CurrentUser, MouseKeyPath, "SmoothMouseYCurve", DefaultSmoothMouseYCurve);

            AuditLogger.Log(AuditLogLevel.Info, "RawMouseInput", "Mouse acceleration restored to Windows defaults.");
            return Task.FromResult(ExecutionResult.Ok("Mouse acceleration restored to Windows defaults."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "RawMouseInput", $"Failed to rollback mouse settings: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }
}
