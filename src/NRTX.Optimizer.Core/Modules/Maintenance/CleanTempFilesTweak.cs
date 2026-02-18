using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Maintenance;

public class CleanTempFilesTweak : ITweak
{
    public string Id => "maintenance.clean_temp_files";
    public string Name => "Clean User & System Temporary Cache Files";
    public string Description => "Purges leftover cache files in %TEMP%, C:\\Windows\\Temp, CrashDumps, and system thumbnail caches.";
    public TweakCategory Category => TweakCategory.Maintenance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    public Task<bool> IsAppliedAsync()
    {
        return Task.FromResult(false);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: %TEMP% and C:\\Windows\\Temp would be cleaned.", isDryRun: true));

        return Task.Run(() =>
        {
            long freedBytes = 0;
            int fileCount = 0;

            var targetDirs = new[]
            {
                Path.GetTempPath(),
                @"C:\Windows\Temp",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"CrashDumps")
            };

            var safeEnumOptions = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                AttributesToSkip = FileAttributes.ReparsePoint
            };

            foreach (var dir in targetDirs)
            {
                if (!Directory.Exists(dir)) continue;

                try
                {
                    var di = new DirectoryInfo(dir);
                    foreach (var file in di.EnumerateFiles("*", safeEnumOptions))
                    {
                        try
                        {
                            long size = file.Length;
                            file.Delete();
                            freedBytes += size;
                            fileCount++;
                        }
                        catch { } // Ignore locked files in use
                    }

                    try
                    {
                        foreach (var subDir in di.EnumerateDirectories("*", new EnumerationOptions { IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint }))
                        {
                            try
                            {
                                subDir.Delete(true);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                catch { }
            }

            var freedMb = Math.Round((double)freedBytes / (1024 * 1024), 2);
            return ExecutionResult.Ok($"Cleaned {fileCount} temporary files ({freedMb} MB freed).");
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        return Task.FromResult(ExecutionResult.Ok("Temp file cleanup is permanent and safe; no rollback needed."));
    }
}
