using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Privacy;

public class DisableFeedbackTrackingTweak : ITweak
{
    public string Id => "privacy.disable_feedback_prompts";
    public string Name => "Disable Windows Feedback Surveys & Diagnostic Notifications";
    public string Description => "Prevents Windows from periodically popping up feedback surveys and diagnostic prompts.";
    public TweakCategory Category => TweakCategory.Privacy;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string SiufUserKey = @"Software\Microsoft\Siuf\Rules";
    private const string PolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection";

    public Task<bool> IsAppliedAsync()
    {
        var val = SafeRegistry.GetDword(RegistryHive.CurrentUser, SiufUserKey, "NumberOfSIUFsInPeriod");
        return Task.FromResult(val == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Feedback prompts would be disabled.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.CurrentUser, SiufUserKey, "NumberOfSIUFsInPeriod", 0);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, PolicyKey, "DoNotShowFeedbackNotifications", 1);

        return Task.FromResult(ExecutionResult.Ok("Feedback prompts and surveys disabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Feedback prompts would be restored.", isDryRun: true));

        SafeRegistry.DeleteValue(RegistryHive.CurrentUser, SiufUserKey, "NumberOfSIUFsInPeriod");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, PolicyKey, "DoNotShowFeedbackNotifications");

        return Task.FromResult(ExecutionResult.Ok("Feedback prompts restored to default."));
    }
}
