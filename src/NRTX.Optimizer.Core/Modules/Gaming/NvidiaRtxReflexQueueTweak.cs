using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

/// <summary>
/// Optimizes NVIDIA Shader Cache disk limits (sets 10GB limit) to eliminate shader compilation stuttering
/// in modern DX11/DX12/Vulkan games like Apex Legends, Warzone, and CS2.
/// </summary>
public class NvidiaRtxReflexQueueTweak : ITweak
{
    public string Id => "gaming.nvidia_rtx_reflex_queue";
    public string Name => "Optimize NVIDIA Shader Cache Limit (10GB) & Direct Flip Queue Presentation";
    public string Description => "Expands DirectX/Vulkan shader cache size to 10GB, eliminating in-game shader compilation stuttering on GTX & RTX cards.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string NvidiaGlobalPath = @"SOFTWARE\NVIDIA Corporation\Global";

    public Task<bool> IsAppliedAsync()
    {
        var cacheSize = SafeRegistry.GetDword(RegistryHive.LocalMachine, NvidiaGlobalPath, "MaxShaderCacheSize");
        return Task.FromResult(cacheSize == 10240);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: NVIDIA Shader Cache would be set to 10GB limit in registry.", isDryRun: true));

        try
        {
            // 10GB Shader Cache limit in Megabytes = 10240 MB
            SafeRegistry.SetDword(RegistryHive.LocalMachine, NvidiaGlobalPath, "MaxShaderCacheSize", 10240);
            SafeRegistry.SetDword(RegistryHive.LocalMachine, NvidiaGlobalPath, "PreRenderedFrames", 1);

            AuditLogger.Log(AuditLogLevel.Success, "NvidiaShaderQueue", "NVIDIA Shader Cache set to 10GB and pre-rendered frames set to 1 (Low Latency).");
            return Task.FromResult(ExecutionResult.Ok("NVIDIA 10GB Shader Cache and low-latency frame queue active."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "NvidiaShaderQueue", $"Failed to configure NVIDIA Shader Cache: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun)
            return Task.FromResult(ExecutionResult.Ok("Dry-run: NVIDIA Shader Cache would be restored to default.", isDryRun: true));

        try
        {
            SafeRegistry.DeleteValue(RegistryHive.LocalMachine, NvidiaGlobalPath, "MaxShaderCacheSize");
            SafeRegistry.DeleteValue(RegistryHive.LocalMachine, NvidiaGlobalPath, "PreRenderedFrames");

            AuditLogger.Log(AuditLogLevel.Info, "NvidiaShaderQueue", "NVIDIA Shader Cache settings rolled back to default.");
            return Task.FromResult(ExecutionResult.Ok("NVIDIA Shader Cache restored to default."));
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "NvidiaShaderQueue", $"Failed to rollback shader cache: {ex.Message}");
            return Task.FromResult(ExecutionResult.Fail(ex.Message));
        }
    }
}
