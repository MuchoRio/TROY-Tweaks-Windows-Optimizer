namespace NRTX.Optimizer.Core.Safety;

public enum AuditLogLevel
{
    Info,
    Warn,
    Error,
    Success
}

public static class AuditLogger
{
    private static readonly object LogLock = new();
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NRTX_Optimizer",
        "Logs"
    );
    private static readonly string LogFilePath = Path.Combine(LogDir, "troy_audit.log");

    public static event Action<string>? OnLogEvent;

    public static string LogPath => LogFilePath;

    public static void EnsureLogDirectory()
    {
        if (!Directory.Exists(LogDir))
        {
            Directory.CreateDirectory(LogDir);
        }
    }

    public static void Log(AuditLogLevel level, string source, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var levelStr = level switch
        {
            AuditLogLevel.Info => "INFO ",
            AuditLogLevel.Warn => "WARN ",
            AuditLogLevel.Error => "ERROR",
            AuditLogLevel.Success => "SUCC ",
            _ => "INFO "
        };

        var logLine = $"[{timestamp}] [{levelStr}] [{source}] {message}";

        try
        {
            lock (LogLock)
            {
                EnsureLogDirectory();
                RotateLogsIfNeeded();
                File.AppendAllText(LogFilePath, logLine + Environment.NewLine);
            }
        }
        catch { }

        OnLogEvent?.Invoke(logLine);
    }

    private static void RotateLogsIfNeeded()
    {
        try
        {
            if (File.Exists(LogFilePath))
            {
                var fi = new FileInfo(LogFilePath);
                if (fi.Length > 5 * 1024 * 1024) // 5 MB
                {
                    var backupPath = Path.Combine(LogDir, $"troy_audit_{DateTime.Now:yyyyMMdd_HHmmss}.old.log");
                    File.Move(LogFilePath, backupPath);
                }
            }
        }
        catch { }
    }
}
