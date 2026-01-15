using System.Diagnostics;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Privacy;

public class DisableTelemetryScheduledTasksTweak : ITweak
{
    public string Id => "privacy.disable_telemetry_scheduled_tasks";
    public string Name => "Disable Windows Telemetry & Diagnostic Scheduled Tasks";
    public string Description => "Disables background scheduled tasks that gather telemetry and trigger heavy CPU disk wakeups (Compatibility Appraiser, CEIP Consolidator, UsbCeip, ProgramDataUpdater).";
    public TweakCategory Category => TweakCategory.Privacy;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private readonly string[] _targetTasks = [
        @"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
        @"\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip",
        @"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
        @"\Microsoft\Windows\Application Experience\ProgramDataUpdater",
        @"\Microsoft\Windows\Autochk\Proxy",
        @"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector"
    ];

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = @"/Query /TN ""\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser""",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                var outText = proc?.StandardOutput.ReadToEnd()?.ToLowerInvariant() ?? "";
                proc?.WaitForExit(3000);

                return outText.Contains("disabled");
            }
            catch
            {
                return false;
            }
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok($"Dry-run: {_targetTasks.Length} Telemetry Scheduled Tasks would be disabled.", isDryRun: true));

        return Task.Run(() =>
        {
            int count = 0;
            foreach (var task in _targetTasks)
            {
                if (RunSchtasks($@"/Change /TN ""{task}"" /Disable"))
                {
                    count++;
                }
            }

            return ExecutionResult.Ok($"Disabled {count} Windows Telemetry Scheduled Tasks successfully.");
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Telemetry Scheduled Tasks would be re-enabled.", isDryRun: true));

        return Task.Run(() =>
        {
            foreach (var task in _targetTasks)
            {
                RunSchtasks($@"/Change /TN ""{task}"" /Enable");
            }

            return ExecutionResult.Ok("Telemetry Scheduled Tasks restored to enabled state.");
        });
    }

    private static bool RunSchtasks(string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
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
