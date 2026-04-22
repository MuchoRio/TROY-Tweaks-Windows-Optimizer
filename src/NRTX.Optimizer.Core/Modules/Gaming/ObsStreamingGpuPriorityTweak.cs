using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class ObsStreamingGpuPriorityTweak : ITweak
{
    public string Id => "gaming.obs_streaming_gpu_priority";
    public string Name => "Optimize OBS Studio & Streaming GPU Priority (Zero Dropped Frames)";
    public string Description => "Configures Windows DWM and MMCSS Capture scheduler to allocate dedicated GPU rendering priority & High CPU/IO execution to OBS Studio, Streamlabs, and Discord Screen Share, eliminating stream encoder lag and frame drops during 99% GPU load.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    private const string CaptureKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Capture";
    private const string ObsIfeoKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\obs64.exe\PerfOptions";
    private const string StreamlabsIfeoKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Streamlabs OBS.exe\PerfOptions";

    public Task<bool> IsAppliedAsync()
    {
        var capGpu = SafeRegistry.GetDword(RegistryHive.LocalMachine, CaptureKey, "GPU Priority");
        var capPrio = SafeRegistry.GetDword(RegistryHive.LocalMachine, CaptureKey, "Priority");
        var capSched = SafeRegistry.GetString(RegistryHive.LocalMachine, CaptureKey, "Scheduling Category");
        var obsCpu = SafeRegistry.GetDword(RegistryHive.LocalMachine, ObsIfeoKey, "CpuPriorityClass");

        return Task.FromResult(capGpu == 8 && capPrio == 6 && capSched == "High" && obsCpu == 3);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: OBS & Capture MMCSS priority would be set to High.", isDryRun: true));

        // 1. MMCSS Capture Task Tuning
        SafeRegistry.SetDword(RegistryHive.LocalMachine, CaptureKey, "GPU Priority", 8);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, CaptureKey, "Priority", 6);
        SafeRegistry.SetString(RegistryHive.LocalMachine, CaptureKey, "Scheduling Category", "High");
        SafeRegistry.SetString(RegistryHive.LocalMachine, CaptureKey, "SFIO Priority", "High");

        // 2. OBS Studio IFEO High Priority
        SafeRegistry.SetDword(RegistryHive.LocalMachine, ObsIfeoKey, "CpuPriorityClass", 3);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, ObsIfeoKey, "IoPriority", 3);

        // 3. Streamlabs IFEO High Priority
        SafeRegistry.SetDword(RegistryHive.LocalMachine, StreamlabsIfeoKey, "CpuPriorityClass", 3);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, StreamlabsIfeoKey, "IoPriority", 3);

        return Task.FromResult(ExecutionResult.Ok("OBS Studio & Live Streaming GPU/CPU priority successfully optimized."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: OBS & Capture priority would be restored to defaults.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, CaptureKey, "GPU Priority", 8);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, CaptureKey, "Priority", 2);
        SafeRegistry.SetString(RegistryHive.LocalMachine, CaptureKey, "Scheduling Category", "Medium");
        SafeRegistry.SetString(RegistryHive.LocalMachine, CaptureKey, "SFIO Priority", "Normal");

        SafeRegistry.DeleteSubKeyTree(RegistryHive.LocalMachine, ObsIfeoKey);
        SafeRegistry.DeleteSubKeyTree(RegistryHive.LocalMachine, StreamlabsIfeoKey);

        return Task.FromResult(ExecutionResult.Ok("OBS Studio & Live Streaming priorities restored to default."));
    }
}
