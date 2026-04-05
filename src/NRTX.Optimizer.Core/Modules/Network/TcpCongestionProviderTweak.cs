using System.Diagnostics;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Network;

public class TcpCongestionProviderTweak : ITweak
{
    public string Id => "network.tcp_congestion_provider";
    public string Name => "Configure Modern Low-Latency TCP Congestion Provider (CTCP / CUBIC)";
    public string Description => "Sets the Windows supplemental TCP congestion control algorithm to CTCP / CUBIC for faster throughput ramp-up and reduced packet queueing delay.";
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
                    Arguments = "int tcp show supplemental",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                var outText = proc?.StandardOutput.ReadToEnd()?.ToLowerInvariant() ?? "";
                proc?.WaitForExit(3000);

                return outText.Contains("ctcp") || outText.Contains("cubic") || outText.Contains("bbr");
            }
            catch
            {
                return false;
            }
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: TCP congestion provider would be set to CTCP/CUBIC.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                // Try CTCP first, then CUBIC
                bool ok = RunNetsh("int tcp set supplemental template=custom congestionprovider=ctcp");
                if (!ok)
                {
                    ok = RunNetsh("int tcp set supplemental template=custom congestionprovider=cubic");
                }

                return ok
                    ? ExecutionResult.Ok("TCP Congestion Provider configured for low latency.")
                    : ExecutionResult.Fail("Failed to configure TCP congestion provider.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Exception while configuring TCP congestion provider.", ex);
            }
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: TCP congestion provider would be restored to default.", isDryRun: true));

        return Task.Run(() =>
        {
            RunNetsh("int tcp set supplemental template=custom congestionprovider=default");
            return ExecutionResult.Ok("TCP Congestion Provider restored to Windows default.");
        });
    }

    private static bool RunNetsh(string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh.exe",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit(3000);
            return proc?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
