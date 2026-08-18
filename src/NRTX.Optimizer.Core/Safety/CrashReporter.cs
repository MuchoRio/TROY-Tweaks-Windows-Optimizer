using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Native;

namespace NRTX.Optimizer.Core.Safety;

/// <summary>
/// Enterprise Diagnostic Crash Reporter & Error Logger.
/// Automatically generates structured, human-readable .txt error report files
/// on both the user's Desktop and LocalAppData CrashReports directory
/// whenever an unhandled exception or critical error occurs.
/// </summary>
public static class CrashReporter
{
    private static readonly string AppErrorDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error");
    private static readonly string LocalAppDataErrorDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NRTX_Optimizer",
        "error"
    );

    public static string GenerateErrorReport(Exception ex, string? sourceContext = null)
    {
        var timestampStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"troy_error_{timestampStr}.txt";

        var sb = new StringBuilder();
        sb.AppendLine("================================================================================");
        sb.AppendLine("  TROY TWEAKS WINDOWS OPTIMIZER - CRASH & ERROR DIAGNOSTIC REPORT");
        sb.AppendLine("================================================================================");
        sb.AppendLine($"Generated At (Local) : {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff zzz}");
        sb.AppendLine($"Generated At (UTC)   : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}Z");
        sb.AppendLine($"Application Version  : 5.4.1 (Community Edition - .NET 10 LTS)");
        sb.AppendLine($"Error Source Context : {sourceContext ?? "Global Error Handler"}");
        sb.AppendLine();

        // 1. System Environment & Hardware Diagnostics
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine("[1] SYSTEM ENVIRONMENT & HARDWARE DIAGNOSTICS");
        sb.AppendLine("--------------------------------------------------------------------------------");
        try
        {
            sb.AppendLine($"Operating System     : {RuntimeInformation.OSDescription} ({GetOsBuildNumber()})");
            sb.AppendLine($"OS Architecture      : {RuntimeInformation.OSArchitecture} (Process: {RuntimeInformation.ProcessArchitecture})");
            sb.AppendLine($"Privilege Level      : {(PrivilegeGuard.IsAdministrator() ? "ELEVATED (ADMINISTRATOR)" : "RESTRICTED (STANDARD USER)")}");
            sb.AppendLine($"Machine Name         : {Environment.MachineName}");
            sb.AppendLine($"User Domain/Name     : {Environment.UserDomainName}\\{Environment.UserName}");
            sb.AppendLine($".NET Runtime Version : {RuntimeInformation.FrameworkDescription}");
            sb.AppendLine($"Logical Processors   : {Environment.ProcessorCount} Cores");
            sb.AppendLine($"Executable Path      : {Environment.ProcessPath ?? AppDomain.CurrentDomain.BaseDirectory}");
            sb.AppendLine($"Working Directory    : {Environment.CurrentDirectory}");

            // RAM Memory Info
            var mem = new NativeMethods.MEMORYSTATUSEX();
            if (NativeMethods.GlobalMemoryStatusEx(mem))
            {
                var totalGb = Math.Round((double)mem.ullTotalPhys / (1024 * 1024 * 1024), 2);
                var availGb = Math.Round((double)mem.ullAvailPhys / (1024 * 1024 * 1024), 2);
                sb.AppendLine($"Memory (RAM)         : {availGb} GB Available / {totalGb} GB Total (Load: {mem.dwMemoryLoad}%)");
            }

            // CPU Name from Registry
            using var cpuKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            var cpuName = cpuKey?.GetValue("ProcessorNameString")?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(cpuName))
            {
                sb.AppendLine($"Processor (CPU)      : {cpuName}");
            }
        }
        catch (Exception diagEx)
        {
            sb.AppendLine($"[!] Failed to query some hardware parameters: {diagEx.Message}");
        }
        sb.AppendLine();

        // 2. Exception Classification & Summary
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine("[2] ERROR CLASSIFICATION & MESSAGE");
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine($"Exception Type       : {ex.GetType().FullName}");
        sb.AppendLine($"Exception Message    : {ex.Message}");
        sb.AppendLine($"Faulting Source      : {ex.Source ?? "Unknown"}");
        sb.AppendLine($"Target Method Site   : {ex.TargetSite?.ToString() ?? "Unknown"}");
        sb.AppendLine($"HResult Code         : 0x{ex.HResult:X8} ({ex.HResult})");
        sb.AppendLine();

        // 3. Inner Exception Chain
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine("[3] INNER EXCEPTION CHAIN");
        sb.AppendLine("--------------------------------------------------------------------------------");
        var inner = ex.InnerException;
        int innerLevel = 1;
        if (inner == null)
        {
            sb.AppendLine("(No inner exceptions detected)");
        }
        else
        {
            while (inner != null)
            {
                sb.AppendLine($"[Inner Level {innerLevel}]");
                sb.AppendLine($"Type    : {inner.GetType().FullName}");
                sb.AppendLine($"Message : {inner.Message}");
                sb.AppendLine($"Source  : {inner.Source}");
                sb.AppendLine($"Site    : {inner.TargetSite}");
                if (!string.IsNullOrWhiteSpace(inner.StackTrace))
                {
                    sb.AppendLine($"Stack   :\n{inner.StackTrace}");
                }
                sb.AppendLine();
                inner = inner.InnerException;
                innerLevel++;
            }
        }
        sb.AppendLine();

        // 4. Full Stack Trace
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine("[4] FULL ERROR STACK TRACE");
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine(string.IsNullOrWhiteSpace(ex.StackTrace) ? "(No stack trace available)" : ex.StackTrace);
        sb.AppendLine();

        // 5. Recent Audit Log Tail (Last 25 Entries)
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine("[5] RECENT AUDIT LOG ENTRIES (LAST 25 LINES)");
        sb.AppendLine("--------------------------------------------------------------------------------");
        try
        {
            var auditPath = AuditLogger.LogPath;
            if (File.Exists(auditPath))
            {
                var lines = File.ReadAllLines(auditPath);
                var takeCount = Math.Min(25, lines.Length);
                for (int i = lines.Length - takeCount; i < lines.Length; i++)
                {
                    sb.AppendLine(lines[i]);
                }
            }
            else
            {
                sb.AppendLine("(Audit log file not yet created)");
            }
        }
        catch (Exception auditEx)
        {
            sb.AppendLine($"[!] Could not read audit log: {auditEx.Message}");
        }
        sb.AppendLine();

        // 6. Support & Repository Footer
        sb.AppendLine("================================================================================");
        sb.AppendLine("  SUPPORT & ISSUE REPORTING");
        sb.AppendLine("  GitHub Repository : https://github.com/MuchoRio/TROY-Tweaks-Windows-Optimizer");
        sb.AppendLine("  Contact Email     : det.rio1337@gmail.com");
        sb.AppendLine("  Please attach this file when filing a bug report or asking for support.");
        sb.AppendLine("================================================================================");

        var reportContent = sb.ToString();
        string primarySavedPath = string.Empty;

        // 1. Save to dedicated Application error/ directory
        try
        {
            if (!Directory.Exists(AppErrorDir))
            {
                Directory.CreateDirectory(AppErrorDir);
            }
            var appPath = Path.Combine(AppErrorDir, fileName);
            File.WriteAllText(appPath, reportContent, Encoding.UTF8);
            primarySavedPath = appPath;
            AuditLogger.Log(AuditLogLevel.Error, "CrashReporter", $"Saved error report to: {appPath}");
        }
        catch { }

        // 2. Also save archive to LocalAppData error/ directory
        try
        {
            if (!Directory.Exists(LocalAppDataErrorDir))
            {
                Directory.CreateDirectory(LocalAppDataErrorDir);
            }
            var localPath = Path.Combine(LocalAppDataErrorDir, fileName);
            File.WriteAllText(localPath, reportContent, Encoding.UTF8);
            if (string.IsNullOrEmpty(primarySavedPath))
            {
                primarySavedPath = localPath;
            }
            AuditLogger.Log(AuditLogLevel.Error, "CrashReporter", $"Saved archive error report to: {localPath}");
        }
        catch { }

        return string.IsNullOrEmpty(primarySavedPath) ? Path.Combine("error", fileName) : primarySavedPath;
    }

    private static string GetOsBuildNumber()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var bNum = key?.GetValue("CurrentBuildNumber")?.ToString();
            var ubr = key?.GetValue("UBR")?.ToString();
            return !string.IsNullOrEmpty(ubr) ? $"{bNum}.{ubr}" : (bNum ?? Environment.OSVersion.Version.ToString());
        }
        catch
        {
            return Environment.OSVersion.Version.ToString();
        }
    }
}
