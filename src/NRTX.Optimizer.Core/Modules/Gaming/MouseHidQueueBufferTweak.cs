using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class MouseHidQueueBufferTweak : ITweak
{
    public string Id => "gaming.mouse_hid_queue_buffer_tuning";
    public string Name => "Expand Mouse Driver HID Data Queue Buffer (1000Hz - 8000Hz Anti-Packet Drop)";
    public string Description => "Expands the mouclass kernel driver MouseDataQueueSize buffer from 100 to 128 packets, preventing buffer overflow and micro-stutter when performing high-speed mouse flicks on high polling rate gaming sensors (1000Hz, 2000Hz, 4000Hz, and 8000Hz).";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string MouclassKey = @"SYSTEM\CurrentControlSet\Services\mouclass\Parameters";

    public Task<bool> IsAppliedAsync()
    {
        var queueSize = SafeRegistry.GetDword(RegistryHive.LocalMachine, MouclassKey, "MouseDataQueueSize");
        return Task.FromResult(queueSize == 128);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: MouseDataQueueSize would be expanded to 128 packets.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, MouclassKey, "MouseDataQueueSize", 128);
        return Task.FromResult(ExecutionResult.Ok("mouclass MouseDataQueueSize buffer expanded to 128 packets."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: MouseDataQueueSize would be restored to default 100.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, MouclassKey, "MouseDataQueueSize", 100);
        return Task.FromResult(ExecutionResult.Ok("mouclass MouseDataQueueSize buffer restored to default 100."));
    }
}
