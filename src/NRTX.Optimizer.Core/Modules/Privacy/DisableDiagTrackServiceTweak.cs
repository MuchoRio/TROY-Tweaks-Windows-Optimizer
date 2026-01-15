using System.ServiceProcess;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Privacy;

public class DisableDiagTrackServiceTweak : ITweak
{
    public string Id => "privacy.disable_diagtrack_service";
    public string Name => "Disable Connected User Experiences & Telemetry Service (DiagTrack)";
    public string Description => "Stops and sets DiagTrack and dmwappushservice startup type to Disabled.";
    public TweakCategory Category => TweakCategory.Privacy;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private readonly string[] _serviceNames = ["DiagTrack", "dmwappushservice"];

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                using var sc = new ServiceController("DiagTrack");
                return sc.StartType == ServiceStartMode.Disabled;
            }
            catch
            {
                return false;
            }
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: DiagTrack and dmwappushservice would be stopped and disabled.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                foreach (var name in _serviceNames)
                {
                    try
                    {
                        using var sc = new ServiceController(name);
                        if (sc.Status == ServiceControllerStatus.Running)
                        {
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                        }
                    }
                    catch { }

                    // Use sc.exe config to safely set start mode
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"config \"{name}\" start=disabled",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    System.Diagnostics.Process.Start(psi)?.WaitForExit(3000);
                }

                return ExecutionResult.Ok("DiagTrack & dmwappushservice stopped and disabled.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Failed to disable DiagTrack service.", ex);
            }
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: DiagTrack service would be set to Automatic.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                foreach (var name in _serviceNames)
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"config \"{name}\" start=auto",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    System.Diagnostics.Process.Start(psi)?.WaitForExit(3000);
                }

                return ExecutionResult.Ok("DiagTrack services restored to Automatic startup.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Failed to restore DiagTrack service.", ex);
            }
        });
    }
}
