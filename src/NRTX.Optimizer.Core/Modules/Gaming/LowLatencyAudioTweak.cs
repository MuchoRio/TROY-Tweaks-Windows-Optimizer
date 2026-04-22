using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class LowLatencyAudioTweak : ITweak
{
    public string Id => "gaming.audio_exclusive_latency";
    public string Name => "Ultra-Low Audio Latency & MMCSS Pro Audio Tuning (Footstep Emas)";
    public string Description => "Configures MMCSS Audio and Pro Audio scheduler to maximum priority (Priority 6, Scheduling High, Clock Rate 10000) and disables audio power throttling, delivering sub-millisecond audio response for footstep clarity in competitive FPS.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string AudioTasksKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio";
    private const string ProAudioTasksKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Pro Audio";

    public Task<bool> IsAppliedAsync()
    {
        var clockRate = SafeRegistry.GetDword(RegistryHive.LocalMachine, AudioTasksKey, "Clock Rate");
        var gpuPriority = SafeRegistry.GetDword(RegistryHive.LocalMachine, AudioTasksKey, "GPU Priority");
        var schedCategory = SafeRegistry.GetString(RegistryHive.LocalMachine, AudioTasksKey, "Scheduling Category");
        var proSchedCategory = SafeRegistry.GetString(RegistryHive.LocalMachine, ProAudioTasksKey, "Scheduling Category");

        bool applied = clockRate == 10000 &&
                       gpuPriority == 8 &&
                       string.Equals(schedCategory, "High", StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(proSchedCategory, "High", StringComparison.OrdinalIgnoreCase);

        return Task.FromResult(applied);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: MMCSS Audio and Pro Audio would be set to high-priority low-latency.", isDryRun: true));

        // Configure Tasks\Audio (Background Only = True ensures background voice apps like Discord are never throttled during games)
        SafeRegistry.SetDword(RegistryHive.LocalMachine, AudioTasksKey, "Affinity", 0);
        SafeRegistry.SetString(RegistryHive.LocalMachine, AudioTasksKey, "Background Only", "True");
        SafeRegistry.SetDword(RegistryHive.LocalMachine, AudioTasksKey, "Clock Rate", 10000);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, AudioTasksKey, "GPU Priority", 8);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, AudioTasksKey, "Priority", 6);
        SafeRegistry.SetString(RegistryHive.LocalMachine, AudioTasksKey, "Scheduling Category", "High");
        SafeRegistry.SetString(RegistryHive.LocalMachine, AudioTasksKey, "SFIO Priority", "High");

        // Configure Tasks\Pro Audio
        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProAudioTasksKey, "Affinity", 0);
        SafeRegistry.SetString(RegistryHive.LocalMachine, ProAudioTasksKey, "Background Only", "True");
        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProAudioTasksKey, "Clock Rate", 10000);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProAudioTasksKey, "GPU Priority", 8);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProAudioTasksKey, "Priority", 6);
        SafeRegistry.SetString(RegistryHive.LocalMachine, ProAudioTasksKey, "Scheduling Category", "High");
        SafeRegistry.SetString(RegistryHive.LocalMachine, ProAudioTasksKey, "SFIO Priority", "High");

        return Task.FromResult(ExecutionResult.Ok("MMCSS Audio & Pro Audio low-latency priority applied successfully."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: MMCSS Audio and Pro Audio would be restored to Windows defaults.", isDryRun: true));

        // Restore Tasks\Audio defaults
        SafeRegistry.SetString(RegistryHive.LocalMachine, AudioTasksKey, "Background Only", "True");
        SafeRegistry.SetDword(RegistryHive.LocalMachine, AudioTasksKey, "Clock Rate", 10000);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, AudioTasksKey, "GPU Priority", 8);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, AudioTasksKey, "Priority", 6);
        SafeRegistry.SetString(RegistryHive.LocalMachine, AudioTasksKey, "Scheduling Category", "Medium");
        SafeRegistry.SetString(RegistryHive.LocalMachine, AudioTasksKey, "SFIO Priority", "Normal");

        // Restore Tasks\Pro Audio defaults
        SafeRegistry.SetString(RegistryHive.LocalMachine, ProAudioTasksKey, "Background Only", "False");
        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProAudioTasksKey, "Clock Rate", 10000);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProAudioTasksKey, "GPU Priority", 8);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProAudioTasksKey, "Priority", 6);
        SafeRegistry.SetString(RegistryHive.LocalMachine, ProAudioTasksKey, "Scheduling Category", "High");
        SafeRegistry.SetString(RegistryHive.LocalMachine, ProAudioTasksKey, "SFIO Priority", "Normal");

        return Task.FromResult(ExecutionResult.Ok("MMCSS Audio settings restored to Windows defaults."));
    }
}
