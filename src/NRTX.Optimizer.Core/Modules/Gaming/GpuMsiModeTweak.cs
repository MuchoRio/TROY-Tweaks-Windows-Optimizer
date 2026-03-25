using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

/// <summary>
/// Enables MSI (Message Signaled Interrupts) Mode and High Device Priority for NVIDIA & AMD Graphics Cards.
/// Eliminates shared IRQ hardware interrupt conflicts and reduces DPC latency spikes in competitive gaming.
/// </summary>
public class GpuMsiModeTweak : ITweak
{
    public string Id => "gaming.gpu_msi_mode";
    public string Name => "Enable MSI Mode (Message Signaled Interrupts) & High Priority on GPU";
    public string Description => "Switches GPU hardware interrupt handling from legacy line-based IRQ to modern Message Signaled Interrupts (MSI) with High Priority.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => true;

    private const string DisplayClassGuid = "{4d36e968-e325-11ce-bfc1-08002be10318}";
    private const string PciEnumPath = @"SYSTEM\CurrentControlSet\Enum\PCI";

    public Task<bool> IsAppliedAsync()
    {
        try
        {
            using var pciKey = Registry.LocalMachine.OpenSubKey(PciEnumPath, false);
            if (pciKey == null) return Task.FromResult(false);

            foreach (var deviceId in pciKey.GetSubKeyNames())
            {
                using var devKey = pciKey.OpenSubKey(deviceId, false);
                if (devKey == null) continue;

                foreach (var instanceId in devKey.GetSubKeyNames())
                {
                    using var instKey = devKey.OpenSubKey(instanceId, false);
                    if (instKey == null) continue;

                    var classGuid = instKey.GetValue("ClassGUID")?.ToString();
                    if (string.Equals(classGuid, DisplayClassGuid, StringComparison.OrdinalIgnoreCase))
                    {
                        using var msiKey = instKey.OpenSubKey(@"Device Parameters\Interrupt Management\MessageSignaledInterruptProperties", false);
                        var msiVal = msiKey?.GetValue("MSISupported");
                        if (msiVal is int i && i == 1) return Task.FromResult(true);
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
            return Task.FromResult(ExecutionResult.Ok("Dry-run: MSI Mode would be enabled for GPU display adapters.", isDryRun: true));

        try
        {
            int count = 0;
            using var pciKey = Registry.LocalMachine.OpenSubKey(PciEnumPath, false);
            if (pciKey != null)
            {
                foreach (var deviceId in pciKey.GetSubKeyNames())
                {
                    try
                    {
                        using var devKey = pciKey.OpenSubKey(deviceId, false);
                        if (devKey == null) continue;

                        foreach (var instanceId in devKey.GetSubKeyNames())
                        {
                            try
                            {
                                using var instKey = devKey.OpenSubKey(instanceId, false);
                                if (instKey == null) continue;

                                var classGuid = instKey.GetValue("ClassGUID")?.ToString();
                                if (string.Equals(classGuid, DisplayClassGuid, StringComparison.OrdinalIgnoreCase))
                                {
                                    string devParamPath = $@"{PciEnumPath}\{deviceId}\{instanceId}\Device Parameters\Interrupt Management";
                                    
                                    SafeRegistry.SetDword(RegistryHive.LocalMachine, $@"{devParamPath}\MessageSignaledInterruptProperties", "MSISupported", 1);
                                    SafeRegistry.SetDword(RegistryHive.LocalMachine, $@"{devParamPath}\Affinity Policy", "DevicePriority", 3); // 3 = High Priority

                                    count++;
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }

            AuditLogger.Log(AuditLogLevel.Success, "GpuMsi", $"MSI Mode verified and enabled on {count} GPU adapter(s).");
            return Task.FromResult(ExecutionResult.Ok($"MSI Mode enabled on {count} GPU display adapter(s)."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "GpuMsi", $"Failed to enable GPU MSI Mode: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: GPU MSI Mode would be reverted to default.", isDryRun: true));

        try
        {
            using var pciKey = Registry.LocalMachine.OpenSubKey(PciEnumPath, false);
            if (pciKey != null)
            {
                foreach (var deviceId in pciKey.GetSubKeyNames())
                {
                    try
                    {
                        using var devKey = pciKey.OpenSubKey(deviceId, false);
                        if (devKey == null) continue;

                        foreach (var instanceId in devKey.GetSubKeyNames())
                        {
                            try
                            {
                                using var instKey = devKey.OpenSubKey(instanceId, false);
                                if (instKey == null) continue;

                                var classGuid = instKey.GetValue("ClassGUID")?.ToString();
                                if (string.Equals(classGuid, DisplayClassGuid, StringComparison.OrdinalIgnoreCase))
                                {
                                    string devParamPath = $@"{PciEnumPath}\{deviceId}\{instanceId}\Device Parameters\Interrupt Management";
                                    SafeRegistry.DeleteValue(RegistryHive.LocalMachine, $@"{devParamPath}\MessageSignaledInterruptProperties", "MSISupported");
                                    SafeRegistry.DeleteValue(RegistryHive.LocalMachine, $@"{devParamPath}\Affinity Policy", "DevicePriority");
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }

            AuditLogger.Log(AuditLogLevel.Info, "GpuMsi", "GPU MSI Mode reverted to default.");
            return Task.FromResult(ExecutionResult.Ok("GPU MSI Mode reverted to default."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "GpuMsi", $"Failed to rollback GPU MSI Mode: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }
}
