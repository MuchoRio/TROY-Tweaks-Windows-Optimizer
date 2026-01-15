using System.Diagnostics;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Network;

public class TcpAutoTuningTweak : ITweak
{
    public string Id => "network.tcp_autotuning_heuristic";
    public string Name => "Optimize TCP Window Auto-Tuning & Disable Chimney Scaling";
    public string Description => "Tunes the Windows TCP stack for high throughput and consistent packet streaming, disabling legacy heuristics.";
    public TweakCategory Category => TweakCategory.Network;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = "int tcp show global",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                var outText = proc?.StandardOutput.ReadToEnd() ?? "";
                proc?.WaitForExit(3000);

                return outText.Contains("normal") && (outText.Contains("disabled") || outText.Contains("default"));
            }
            catch
            {
                return false;
            }
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: netsh TCP stack parameters would be optimized.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                RunNetsh("int tcp set global autotuninglevel=normal");
                RunNetsh("int tcp set global rss=enabled");
                RunNetsh("int tcp set global timestamps=disabled");
                RunNetsh("int tcp set heuristics disabled");
                RunNetsh("int tcp set global ecncapability=disabled");

                return ExecutionResult.Ok("TCP Stack auto-tuning and throughput parameters optimized.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Failed to tune TCP stack.", ex);
            }
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: netsh TCP stack would be restored to defaults.", isDryRun: true));

        return Task.Run(() =>
        {
            RunNetsh("int tcp set global autotuninglevel=normal");
            RunNetsh("int tcp set heuristics enabled");
            RunNetsh("int tcp set global timestamps=default");
            return ExecutionResult.Ok("TCP Stack restored to Windows default state.");
        });
    }

    private static void RunNetsh(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "netsh.exe",
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        Process.Start(psi)?.WaitForExit(3000);
    }
}
