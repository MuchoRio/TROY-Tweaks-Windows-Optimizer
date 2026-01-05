namespace NRTX.Optimizer.Core.Models;

public enum TweakCategory
{
    Privacy,
    Performance,
    Gaming,
    Network,
    Services,
    Debloater,
    Maintenance
}

public enum RiskLevel
{
    Safe,          // 100% safe for all users, zero chance of breaking daily apps
    Recommended,   // Recommended for gamers, devs, power users; tested and stable
    Advanced       // For advanced users only; may affect specific niche features
}

public enum TweakState
{
    Unknown,
    NotApplied,
    Applied,
    Error
}

public class ExecutionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsDryRun { get; set; }

    public static ExecutionResult Ok(string message, string? details = null, bool isDryRun = false)
        => new() { Success = true, Message = message, Details = details, IsDryRun = isDryRun };

    public static ExecutionResult Fail(string message, Exception? ex = null, string? details = null)
        => new() { Success = false, Message = message, Details = details, Exception = ex };
}
