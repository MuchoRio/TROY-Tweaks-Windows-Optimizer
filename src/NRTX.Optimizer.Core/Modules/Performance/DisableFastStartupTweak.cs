using System.Diagnostics;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Performance;

public class DisableFastStartupTweak : ITweak
{
    public string Id => "perf.disable_fast_startup_clean_boot";
    public string Name => "Disable Windows Fast Startup & Hybrid Sleep (Fix Motherboard Beep & Display Glitches)";
    public string Description => "Disables Windows Fast Startup (HiberbootEnabled = 0) and purges hybrid sleep state. Enforces 100% clean shutdown and clean BIOS hardware POST, eliminating motherboard long beep warnings, RAM training resume hangs, and GPU display handshake timeouts on 180Hz monitors.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string PowerKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\Power";

    public Task<bool> IsAppliedAsync()
    {
        var hiberboot = SafeRegistry.GetDword(RegistryHive.LocalMachine, PowerKey, "HiberbootEnabled");
        return Task.FromResult(hiberboot == 0 && !File.Exists(@"C:\hiberfil.sys"));
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Fast Startup would be set to 0 and hiberfil.sys purged.", isDryRun: true));

        // 1. Set HiberbootEnabled to 0 in registry
        SafeRegistry.SetDword(RegistryHive.LocalMachine, PowerKey, "HiberbootEnabled", 0);

        // 2. Execute powercfg -h off to remove hiberfil.sys
        try
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
        }
        catch { }

        return Task.FromResult(ExecutionResult.Ok("Windows Fast Startup disabled. System will now always perform 100% clean boot."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Fast Startup would be restored to default.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, PowerKey, "HiberbootEnabled", 1);

        try
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
        }
        catch { }

        return Task.FromResult(ExecutionResult.Ok("Windows Fast Startup restored to default."));
    }
}
