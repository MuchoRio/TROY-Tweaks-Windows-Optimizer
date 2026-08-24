using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Performance;

/// <summary>
/// Enables MSI (Message Signaled Interrupts) Mode and High Priority for NVMe, PCIe, and SATA Storage Controllers.
/// Reduces disk I/O latency and prevents DPC spikes during heavy game asset loading.
/// </summary>
public class StorageMsiModeTweak : ITweak
{
    public string Id => "perf.nvme_storage_msi_mode";
    public string Name => "Enable MSI Mode & High Priority for NVMe/SATA Storage Controllers";
    public string Description => "Enables Message Signaled Interrupts (MSI) on NVMe, PCIe SSD, and SATA storage controllers for faster disk I/O and reduced interrupt latency.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => true;

    private const string StorageClassGuid = "{4d36e97b-e325-11ce-bfc1-08002be10318}"; // SCSI/RAID/NVMe Controller
    private const string HdcClassGuid = "{4d36e96a-e325-11ce-bfc1-08002be10318}";     // IDE/AHCI Controller
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
                    if (string.Equals(classGuid, StorageClassGuid, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(classGuid, HdcClassGuid, StringComparison.OrdinalIgnoreCase))
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
            return Task.FromResult(ExecutionResult.Ok("Dry-run: MSI Mode would be enabled for storage controllers.", isDryRun: true));

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
                                if (string.Equals(classGuid, StorageClassGuid, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(classGuid, HdcClassGuid, StringComparison.OrdinalIgnoreCase))
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

            AuditLogger.Log(AuditLogLevel.Success, "StorageMsi", $"MSI Mode enabled on {count} storage controller(s).");
            return Task.FromResult(ExecutionResult.Ok($"MSI Mode enabled on {count} storage controller(s)."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "StorageMsi", $"Failed to enable Storage MSI Mode: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: Storage MSI Mode would be reverted to default.", isDryRun: true));

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
                                if (string.Equals(classGuid, StorageClassGuid, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(classGuid, HdcClassGuid, StringComparison.OrdinalIgnoreCase))
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

            AuditLogger.Log(AuditLogLevel.Info, "StorageMsi", "Storage MSI Mode reverted to default.");
            return Task.FromResult(ExecutionResult.Ok("Storage MSI Mode reverted to default."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "StorageMsi", $"Failed to rollback Storage MSI Mode: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }
}
