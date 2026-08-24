using System.Diagnostics;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Performance;

public class DisableHibernationTweak : ITweak
{
    public string Id => "perf.disable_hibernation";
    public string Name => "Disable Hibernation & Delete hiberfil.sys (Free 8-32GB Storage)";
    public string Description => "Disables Windows Hibernation file (hiberfil.sys) reclaiming SSD space equivalent to 75-100% of your total RAM size.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() => !File.Exists(@"C:\hiberfil.sys"));
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Hibernation would be disabled via powercfg /h off.", isDryRun: true));

        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powercfg.exe",
                Arguments = "-h off",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit(5000);
            return proc?.ExitCode == 0
                ? ExecutionResult.Ok("Hibernation disabled and hiberfil.sys removed.")
                : ExecutionResult.Fail("Failed to disable hibernation.");
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Hibernation would be re-enabled.", isDryRun: true));

        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powercfg.exe",
                Arguments = "-h on",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit(5000);
            return ExecutionResult.Ok("Hibernation re-enabled.");
        });
    }
}
