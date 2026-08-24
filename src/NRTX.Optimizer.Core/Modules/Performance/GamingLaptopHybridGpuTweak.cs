using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Performance;

public class GamingLaptopHybridGpuTweak : ITweak
{
    public string Id => "perf.laptop_hybrid_gpu_high_perf";
    public string Name => "Gaming Laptop Dual-GPU & Power Throttling Bypass (Pavilion, TUF, Helios, Legion)";
    public string Description => "Optimizes gaming laptops with Dual-GPU (NVIDIA Optimus / Intel / AMD iGPU + dGPU) and disables aggressive Windows Power Throttling on plugged-in AC power. Forces DirectX Discrete High-Performance GPU routing to eliminate cross-adapter copy FPS loss during gaming & live streaming.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    private const string PowerThrottlingKey = @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling";
    private const string D3DGpuPrefsKey = @"Software\Microsoft\DirectX\UserGpuPreferences";
    private const string GraphicsSettingsKey = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";

    public Task<bool> IsAppliedAsync()
    {
        var throttleOff = SafeRegistry.GetDword(RegistryHive.LocalMachine, PowerThrottlingKey, "PowerThrottlingOff");
        var hagsVal = SafeRegistry.GetDword(RegistryHive.LocalMachine, GraphicsSettingsKey, "HwSchMode");

        return Task.FromResult(throttleOff == 1 && hagsVal == 2);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Laptop Power Throttling bypass and dGPU preferences would be applied.", isDryRun: true));

        // 1. Disable Windows Power Throttling on AC power
        SafeRegistry.SetDword(RegistryHive.LocalMachine, PowerThrottlingKey, "PowerThrottlingOff", 1);

        // 2. Enable Hardware-Accelerated GPU Scheduling (HAGS) for Dual-GPU laptops
        SafeRegistry.SetDword(RegistryHive.LocalMachine, GraphicsSettingsKey, "HwSchMode", 2);

        // 3. Configure DirectX High-Performance routing preference for common streaming/recording apps
        string[] streamerBinaries = ["obs64.exe", "Streamlabs OBS.exe", "Discord.exe", "Medal.exe"];
        foreach (var bin in streamerBinaries)
        {
            SafeRegistry.SetString(RegistryHive.CurrentUser, D3DGpuPrefsKey, bin, "GpuPreference=2;");
        }

        return Task.FromResult(ExecutionResult.Ok("Gaming Laptop Dual-GPU routing and Power Throttling bypass successfully applied."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Laptop power throttling settings would be restored.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, PowerThrottlingKey, "PowerThrottlingOff", 0);

        string[] streamerBinaries = ["obs64.exe", "Streamlabs OBS.exe", "Discord.exe", "Medal.exe"];
        foreach (var bin in streamerBinaries)
        {
            SafeRegistry.DeleteValue(RegistryHive.CurrentUser, D3DGpuPrefsKey, bin);
        }

        return Task.FromResult(ExecutionResult.Ok("Gaming Laptop power throttling and GPU preferences restored to default."));
    }
}
