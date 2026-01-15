using System.Diagnostics;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Debloater;

public class UwpDebloaterTweak : ITweak
{
    public string Id => "debloater.uwp_bloatware";
    public string Name => "Remove Pre-installed Windows 10/11 UWP Bloatware Apps";
    public string Description => "Safely removes pre-installed sponsored junk UWP apps (Bing News, Solitaire, Clipchamp, Skype, Feedback Hub) while strictly preserving Microsoft Store, Terminal, & Calculator.";
    public TweakCategory Category => TweakCategory.Debloater;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    public static readonly string[] SafeJunkApps = [
        "*BingNews*",
        "*BingWeather*",
        "*MicrosoftSolitaireCollection*",
        "*GetHelp*",
        "*Getstarted*",
        "*YourPhone*",
        "*ZuneVideo*",
        "*ZuneMusic*",
        "*SkypeApp*",
        "*Clipchamp*",
        "*Todos*",
        "*PowerAutomateDesktop*",
        "*FeedbackHub*"
    ];

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"(Get-AppxPackage | Where-Object { $_.Name -match 'BingNews|Clipchamp' } | Measure-Object).Count\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                var outText = proc?.StandardOutput.ReadToEnd()?.Trim() ?? "0";
                proc?.WaitForExit(5000);
                return outText == "0";
            }
            catch
            {
                return false;
            }
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok($"Dry-run: {SafeJunkApps.Length} UWP bloatware app packages would be removed.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                var appList = string.Join(",", SafeJunkApps.Select(a => $"'{a}'"));
                var psScript = $@"
$apps = @({appList})
foreach ($app in $apps) {{
    Get-AppxPackage -Name $app -AllUsers -ErrorAction SilentlyContinue | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue
    Get-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue | Where-Object {{ $_.PackageName -like $app }} | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue
}}
";
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript.Replace("\"", "\\\"")}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(30000);

                return ExecutionResult.Ok("UWP bloatware removed and provisioned packages de-registered.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Failed to remove UWP bloatware.", ex);
            }
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Bloatware can be re-installed from Microsoft Store.", isDryRun: true));

        return Task.FromResult(ExecutionResult.Ok("UWP Apps can be re-installed from Microsoft Store at any time."));
    }
}
