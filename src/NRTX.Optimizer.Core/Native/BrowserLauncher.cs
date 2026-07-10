using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Native;

/// <summary>
/// High-Reliability Default Browser Launcher for Windows.
/// Reliably detects and opens URLs directly in the user's configured default web browser
/// even when running elevated under Windows UAC / Administrator tokens.
/// </summary>
public static class BrowserLauncher
{
    public const string DefaultVipUrl = "https://github.com/nrtxlabs/";

    public static void OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        Task.Run(() =>
        {
            // Method 1: Launch Default Browser Executable directly from Registry UserChoice
            // (100% reliable even under Elevated Admin / UAC tokens)
            try
            {
                var browserExe = GetDefaultBrowserPath();
                if (!string.IsNullOrEmpty(browserExe) && File.Exists(browserExe))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = browserExe,
                        Arguments = $"\"{url}\"",
                        UseShellExecute = false
                    };
                    Process.Start(startInfo);
                    AuditLogger.Log(AuditLogLevel.Info, "Browser", $"Successfully launched default browser ({Path.GetFileName(browserExe)}): {url}");
                    return;
                }
            }
            catch (Exception ex1)
            {
                AuditLogger.Log(AuditLogLevel.Warn, "Browser", $"Direct default browser launch failed: {ex1.Message}. Falling back to cmd start...");
            }

            // Method 2: cmd.exe start (spawns shell association)
            try
            {
                var cmdInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"\" \"{url}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(cmdInfo);
                AuditLogger.Log(AuditLogLevel.Info, "Browser", $"Successfully launched URL via cmd.exe: {url}");
                return;
            }
            catch (Exception ex2)
            {
                AuditLogger.Log(AuditLogLevel.Warn, "Browser", $"cmd.exe start failed: {ex2.Message}. Falling back to rundll32...");
            }

            // Method 3: rundll32 url.dll,FileProtocolHandler
            try
            {
                var runDllInfo = new ProcessStartInfo
                {
                    FileName = "rundll32.exe",
                    Arguments = $"url.dll,FileProtocolHandler \"{url}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(runDllInfo);
                AuditLogger.Log(AuditLogLevel.Info, "Browser", $"Successfully launched URL via rundll32: {url}");
                return;
            }
            catch (Exception ex3)
            {
                AuditLogger.Log(AuditLogLevel.Warn, "Browser", $"rundll32 failed: {ex3.Message}. Falling back to installed browser scan...");
            }

            // Method 4: Scan popular installed browser locations
            string[] candidatePaths = [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\Edge\Application\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Google\Chrome\Application\chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Mozilla Firefox\firefox.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"BraveSoftware\Brave-Browser\Application\brave.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Opera\launcher.exe")
            ];

            foreach (var path in candidatePaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = path,
                            Arguments = $"\"{url}\"",
                            UseShellExecute = false
                        });
                        AuditLogger.Log(AuditLogLevel.Info, "Browser", $"Successfully launched via fallback browser ({Path.GetFileName(path)}): {url}");
                        return;
                    }
                }
                catch { }
            }

            // Method 5: Standard ShellExecute fallback
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                AuditLogger.Log(AuditLogLevel.Info, "Browser", $"Successfully launched URL via ShellExecute: {url}");
            }
            catch (Exception exFinal)
            {
                AuditLogger.Log(AuditLogLevel.Error, "Browser", $"All browser launch methods failed for {url}: {exFinal.Message}");
            }
        });
    }

    /// <summary>
    /// Reads user's default browser executable path directly from Windows Registry UserChoice.
    /// Supports Chrome, Edge, Firefox, Brave, Opera, Vivaldi, Arc, etc.
    /// </summary>
    public static string? GetDefaultBrowserPath()
    {
        try
        {
            // 1. Query HKCU https association
            using var userChoiceKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice");
            var progId = userChoiceKey?.GetValue("ProgId")?.ToString();

            // Fallback to http association if https is not explicitly present
            if (string.IsNullOrEmpty(progId))
            {
                using var httpChoiceKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice");
                progId = httpChoiceKey?.GetValue("ProgId")?.ToString();
            }

            if (string.IsNullOrEmpty(progId)) return null;

            // 2. Query HKCR\<ProgId>\shell\open\command for actual executable
            using var cmdKey = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
            var rawCmd = cmdKey?.GetValue(null)?.ToString();
            if (string.IsNullOrEmpty(rawCmd)) return null;

            return ExtractExecutablePath(rawCmd);
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractExecutablePath(string rawCommand)
    {
        if (string.IsNullOrWhiteSpace(rawCommand)) return null;

        string trimmed = rawCommand.Trim();
        if (trimmed.StartsWith("\""))
        {
            int endQuote = trimmed.IndexOf('\"', 1);
            if (endQuote > 1)
            {
                return trimmed.Substring(1, endQuote - 1);
            }
        }

        int spaceIndex = trimmed.IndexOf(' ');
        return spaceIndex > 0 ? trimmed.Substring(0, spaceIndex) : trimmed;
    }
}
