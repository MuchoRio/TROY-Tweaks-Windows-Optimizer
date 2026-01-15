using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

/// <summary>
/// Disables AMD Radeon Ultra Low Power State (ULPS) to eliminate GPU clock drops and micro-stuttering
/// during high-framerate competitive gaming.
/// </summary>
public class AmdRadeonAntiLagUlpsTweak : ITweak
{
    public string Id => "gaming.amd_radeon_anti_lag_ulps";
    public string Name => "Disable AMD Radeon Ultra Low Power State (ULPS) & Anti-Lag Boost";
    public string Description => "Disables ULPS in AMD Radeon display driver registry, preventing sudden core clock downthrottling during intensive FPS matches.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => true;

    private const string VideoClassPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";

    public Task<bool> IsAppliedAsync()
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey(VideoClassPath, false);
            if (baseKey != null)
            {
                foreach (var subName in baseKey.GetSubKeyNames())
                {
                    if (subName.StartsWith("000", StringComparison.OrdinalIgnoreCase))
                    {
                        using var sub = baseKey.OpenSubKey(subName, false);
                        var val = sub?.GetValue("EnableUlps");
                        if (val is int i && i == 0) return Task.FromResult(true);
                    }
                }
            }
        }
        catch
        {
            // Ignore
        }

        return Task.FromResult(false);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: AMD Radeon ULPS would be set to 0 across all video class adapters.", isDryRun: true));

        try
        {
            int modifiedCount = 0;
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey(VideoClassPath, false);
            if (baseKey != null)
            {
                foreach (var subName in baseKey.GetSubKeyNames())
                {
                    if (subName.StartsWith("000", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            using var sub = baseKey.OpenSubKey(subName, false);
                            if (sub != null && sub.GetValue("EnableUlps") != null)
                            {
                                SafeRegistry.SetDword(RegistryHive.LocalMachine, $@"{VideoClassPath}\{subName}", "EnableUlps", 0);
                                SafeRegistry.SetDword(RegistryHive.LocalMachine, $@"{VideoClassPath}\{subName}", "EnableUlps_NA", 0);
                                modifiedCount++;
                            }
                        }
                        catch { }
                    }
                }
            }

            AuditLogger.Log(AuditLogLevel.Success, "AmdUlps", $"AMD Radeon ULPS disabled across {modifiedCount} driver instance(s).");
            return Task.FromResult(ExecutionResult.Ok($"AMD Radeon ULPS disabled on {modifiedCount} driver instance(s)."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "AmdUlps", $"Failed to disable AMD ULPS: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: AMD Radeon ULPS would be re-enabled.", isDryRun: true));

        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey(VideoClassPath, false);
            if (baseKey != null)
            {
                foreach (var subName in baseKey.GetSubKeyNames())
                {
                    if (subName.StartsWith("000", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            using var sub = baseKey.OpenSubKey(subName, false);
                            if (sub != null && sub.GetValue("EnableUlps") != null)
                            {
                                SafeRegistry.SetDword(RegistryHive.LocalMachine, $@"{VideoClassPath}\{subName}", "EnableUlps", 1);
                            }
                        }
                        catch { }
                    }
                }
            }

            AuditLogger.Log(AuditLogLevel.Info, "AmdUlps", "AMD Radeon ULPS re-enabled to default.");
            return Task.FromResult(ExecutionResult.Ok("AMD Radeon ULPS re-enabled to default."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "AmdUlps", $"Failed to rollback AMD ULPS: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }
}
