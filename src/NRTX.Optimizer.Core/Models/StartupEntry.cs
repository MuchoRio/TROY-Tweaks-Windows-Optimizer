namespace NRTX.Optimizer.Core.Models;

public enum StartupLocation
{
    CurrentUserRegistry,
    LocalMachineRegistry,
    LocalMachineWow64Registry,
    UserStartupFolder,
    CommonStartupFolder,
    TaskSchedulerLogon
}

public enum StartupImpact
{
    Low,
    Medium,
    High
}

public class StartupEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public StartupLocation Location { get; set; }
    public bool IsEnabled { get; set; } = true;
    public StartupImpact Impact { get; set; } = StartupImpact.Low;
    public string RawLocationPath { get; set; } = string.Empty;
}
