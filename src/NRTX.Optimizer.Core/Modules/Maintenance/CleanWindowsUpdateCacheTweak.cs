using System.Diagnostics;
using System.ServiceProcess;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Maintenance;

public class CleanWindowsUpdateCacheTweak : ITweak
{
    public string Id => "maintenance.clean_windows_update_cache";
    public string Name => "Clean Windows Update Download Cache (SoftwareDistribution)";
    public string Description => "Safely stops wuauserv, purges old downloaded update installer leftovers in SoftwareDistribution\\Download, and restarts the update service.";
    public TweakCategory Category => TweakCategory.Maintenance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    public Task<bool> IsAppliedAsync()
    {
        return Task.FromResult(false);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Windows Update download cache would be cleared.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                // 1. Stop wuauserv & bits
                RunSc("stop \"wuauserv\"");
                RunSc("stop \"bits\"");
                Thread.Sleep(1000);

                // 2. Delete Download folder contents
                long freedBytes = 0;
                int count = 0;
                var downloadPath = @"C:\Windows\SoftwareDistribution\Download";
                if (Directory.Exists(downloadPath))
                {
                    var di = new DirectoryInfo(downloadPath);
                    var safeOptions = new EnumerationOptions
                    {
                        IgnoreInaccessible = true,
                        RecurseSubdirectories = true,
                        AttributesToSkip = FileAttributes.ReparsePoint
                    };
                    foreach (var file in di.EnumerateFiles("*", safeOptions))
                    {
                        try
                        {
                            long len = file.Length;
                            file.Delete();
                            freedBytes += len;
                            count++;
                        }
                        catch { }
                    }
                }

                // 3. Restart wuauserv
                RunSc("start \"wuauserv\"");

                var freedMb = Math.Round((double)freedBytes / (1024 * 1024), 2);
                return ExecutionResult.Ok($"Windows Update cache purged: {count} files ({freedMb} MB freed).");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Failed to clean Windows Update cache.", ex);
            }
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        return Task.FromResult(ExecutionResult.Ok("Update cache purge is a one-time maintenance operation; no rollback needed."));
    }

    private static void RunSc(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "sc.exe",
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var proc = Process.Start(psi);
        proc?.WaitForExit(3000);
    }
}
