using System.Diagnostics;
using System.ServiceProcess;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Services;

public class OptimizeWindowsServicesTweak : ITweak
{
    public string Id => "services.optimize_background_junk";
    public string Name => "Disable Unnecessary Background Bloat Services (Maps, RetailDemo, WER)";
    public string Description => "Disables non-essential background services (MapsBroker, RetailDemo, WerSvc, wisvc, TroubleshootingSvc) to save RAM and reduce CPU background wakeups.";
    public TweakCategory Category => TweakCategory.Services;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private readonly string[] _targetServices = [
        "MapsBroker",
        "RetailDemo",
        "wisvc",
        "WerSvc",
        "TroubleshootingSvc"
    ];

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                int disabledCount = 0;
                foreach (var svc in _targetServices)
                {
                    try
                    {
                        using var sc = new ServiceController(svc);
                        if (sc.StartType == ServiceStartMode.Disabled)
                        {
                            disabledCount++;
                        }
                    }
                    catch { }
                }
                return disabledCount >= 3;
            }
            catch
            {
                return false;
            }
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Non-essential bloat services would be stopped and disabled.", isDryRun: true));

        return Task.Run(() =>
        {
            int modified = 0;
            foreach (var svc in _targetServices)
            {
                try
                {
                    using var sc = new ServiceController(svc);
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                    }
                }
                catch { }

                var psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"config \"{svc}\" start=disabled",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(3000);
                if (proc?.ExitCode == 0) modified++;
            }

            return ExecutionResult.Ok($"Optimized {modified} background services successfully.");
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Background services would be restored to Manual/Auto.", isDryRun: true));

        return Task.Run(() =>
        {
            foreach (var svc in _targetServices)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"config \"{svc}\" start=demand",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(3000);
            }

            return ExecutionResult.Ok("Background services restored to Manual (Demand) start.");
        });
    }
}
