using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Performance;

public class DisableNtfs8dot3NameCreationTweak : ITweak
{
    public string Id => "perf.disable_ntfs_8dot3_and_last_access";
    public string Name => "Disable NTFS 8.3 Name Creation & Last Access Update (SSD & NVMe Boost)";
    public string Description => "Disables legacy MS-DOS 8.3 short filename generation and file read timestamp updates on NTFS volumes, reducing disk I/O overhead and prolonging SSD endurance.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string FileSystemKey = @"SYSTEM\CurrentControlSet\Control\FileSystem";

    public Task<bool> IsAppliedAsync()
    {
        var shortNames = SafeRegistry.GetDword(RegistryHive.LocalMachine, FileSystemKey, "NtfsDisable8dot3NameCreation");
        var lastAccess = SafeRegistry.GetDword(RegistryHive.LocalMachine, FileSystemKey, "NtfsDisableLastAccessUpdate");
        return Task.FromResult(shortNames == 1 && lastAccess == 1);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: NTFS 8.3 short names and last access updates would be disabled.", isDryRun: true));

        RegistryBackupEngine.BackupKey(@"HKEY_LOCAL_MACHINE\" + FileSystemKey, "ntfs_filesystem");

        bool ok1 = SafeRegistry.SetDword(RegistryHive.LocalMachine, FileSystemKey, "NtfsDisable8dot3NameCreation", 1);
        bool ok2 = SafeRegistry.SetDword(RegistryHive.LocalMachine, FileSystemKey, "NtfsDisableLastAccessUpdate", 1);

        return Task.FromResult(ok1 && ok2
            ? ExecutionResult.Ok("NTFS 8.3 short names and last access updates disabled.")
            : ExecutionResult.Fail("Failed to update NTFS FileSystem parameters."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: NTFS FileSystem settings would be restored to defaults.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, FileSystemKey, "NtfsDisable8dot3NameCreation", 2); // Default is 2 (Volume-level configuration)
        SafeRegistry.SetDword(RegistryHive.LocalMachine, FileSystemKey, "NtfsDisableLastAccessUpdate", 2);

        return Task.FromResult(ExecutionResult.Ok("NTFS FileSystem settings restored to Windows defaults."));
    }
}
