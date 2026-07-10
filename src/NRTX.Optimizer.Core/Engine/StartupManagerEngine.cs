using System.Diagnostics;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Engine;

public static class StartupManagerEngine
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunDisabledKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run\AutorunsDisabled";
    private const string WowRunKeyPath = @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run";
    private const string WowRunDisabledKeyPath = @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run\AutorunsDisabled";

    public static async Task<List<StartupEntry>> GetStartupEntriesAsync(CancellationToken cancellationToken = default)
    {
        var hkcuTask = Task.Run(() =>
        {
            var list = new List<StartupEntry>();
            ScanRegistryHive(Registry.CurrentUser, RunKeyPath, StartupLocation.CurrentUserRegistry, isEnabled: true, list);
            ScanRegistryHive(Registry.CurrentUser, RunDisabledKeyPath, StartupLocation.CurrentUserRegistry, isEnabled: false, list);
            return list;
        }, cancellationToken);

        var hklmTask = Task.Run(() =>
        {
            var list = new List<StartupEntry>();
            ScanRegistryHive(Registry.LocalMachine, RunKeyPath, StartupLocation.LocalMachineRegistry, isEnabled: true, list);
            ScanRegistryHive(Registry.LocalMachine, RunDisabledKeyPath, StartupLocation.LocalMachineRegistry, isEnabled: false, list);
            return list;
        }, cancellationToken);

        var wowTask = Task.Run(() =>
        {
            var list = new List<StartupEntry>();
            ScanRegistryHive(Registry.LocalMachine, WowRunKeyPath, StartupLocation.LocalMachineWow64Registry, isEnabled: true, list);
            ScanRegistryHive(Registry.LocalMachine, WowRunDisabledKeyPath, StartupLocation.LocalMachineWow64Registry, isEnabled: false, list);
            return list;
        }, cancellationToken);

        var userFolderTask = Task.Run(() =>
        {
            var list = new List<StartupEntry>();
            var userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            ScanFolder(userStartup, StartupLocation.UserStartupFolder, list);
            return list;
        }, cancellationToken);

        var commonFolderTask = Task.Run(() =>
        {
            var list = new List<StartupEntry>();
            var commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            ScanFolder(commonStartup, StartupLocation.CommonStartupFolder, list);
            return list;
        }, cancellationToken);

        var taskSchedTask = Task.Run(() =>
        {
            var list = new List<StartupEntry>();
            ScanScheduledTasks(list);
            return list;
        }, cancellationToken);

        var results = await Task.WhenAll(
            hkcuTask, hklmTask, wowTask, userFolderTask, commonFolderTask, taskSchedTask
        );

        return results.SelectMany(r => r).ToList();
    }

    public static bool ToggleStartupEntry(StartupEntry entry, bool enable)
    {
        try
        {
            if (entry.Location == StartupLocation.CurrentUserRegistry ||
                entry.Location == StartupLocation.LocalMachineRegistry ||
                entry.Location == StartupLocation.LocalMachineWow64Registry)
            {
                var hive = entry.Location == StartupLocation.CurrentUserRegistry ? Registry.CurrentUser : Registry.LocalMachine;
                string activePath = entry.Location == StartupLocation.LocalMachineWow64Registry ? WowRunKeyPath : RunKeyPath;
                string disabledPath = entry.Location == StartupLocation.LocalMachineWow64Registry ? WowRunDisabledKeyPath : RunDisabledKeyPath;

                if (enable)
                {
                    // Move from Disabled to Active
                    using var disKey = hive.OpenSubKey(disabledPath, true);
                    var cmd = disKey?.GetValue(entry.Name);
                    if (cmd != null)
                    {
                        using var actKey = hive.CreateSubKey(activePath, true);
                        actKey?.SetValue(entry.Name, cmd);
                        disKey?.DeleteValue(entry.Name, false);
                        entry.IsEnabled = true;
                        AuditLogger.Log(AuditLogLevel.Info, "StartupManager", $"Toggled startup entry '{entry.Name}' to Enabled.");
                        return true;
                    }
                    return false;
                }
                else
                {
                    // Move from Active to Disabled
                    using var actKey = hive.OpenSubKey(activePath, true);
                    var cmd = actKey?.GetValue(entry.Name);
                    if (cmd != null)
                    {
                        using var disKey = hive.CreateSubKey(disabledPath, true);
                        disKey?.SetValue(entry.Name, cmd);
                        actKey?.DeleteValue(entry.Name, false);
                        entry.IsEnabled = false;
                        AuditLogger.Log(AuditLogLevel.Info, "StartupManager", $"Toggled startup entry '{entry.Name}' to Disabled.");
                        return true;
                    }
                    return false;
                }
            }
            else if (entry.Location == StartupLocation.UserStartupFolder || entry.Location == StartupLocation.CommonStartupFolder)
            {
                if (File.Exists(entry.RawLocationPath))
                {
                    if (!enable && !entry.RawLocationPath.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase))
                    {
                        string target = entry.RawLocationPath + ".disabled";
                        File.Move(entry.RawLocationPath, target, true);
                        entry.RawLocationPath = target;
                        entry.IsEnabled = false;
                        return true;
                    }
                    else if (enable && entry.RawLocationPath.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase))
                    {
                        string target = entry.RawLocationPath[..^9]; // Strip .disabled
                        File.Move(entry.RawLocationPath, target, true);
                        entry.RawLocationPath = target;
                        entry.IsEnabled = true;
                        return true;
                    }
                }
            }
            else if (entry.Location == StartupLocation.TaskSchedulerLogon)
            {
                string flag = enable ? "/enable" : "/disable";
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/change /tn \"{entry.RawLocationPath}\" {flag}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    proc.WaitForExit(3000);
                    if (proc.ExitCode == 0)
                    {
                        entry.IsEnabled = enable;
                        AuditLogger.Log(AuditLogLevel.Info, "StartupManager", $"Toggled scheduled task '{entry.Name}' to {(enable ? "Enabled" : "Disabled")}.");
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "StartupManager", $"Failed to toggle startup entry '{entry.Name}': {ex.Message}");
        }

        return false;
    }

    private static void ScanRegistryHive(RegistryKey hive, string subKeyPath, StartupLocation location, bool isEnabled, List<StartupEntry> entries)
    {
        try
        {
            using var key = hive.OpenSubKey(subKeyPath, false);
            if (key == null) return;

            foreach (var valName in key.GetValueNames())
            {
                if (string.IsNullOrWhiteSpace(valName)) continue;
                var cmd = key.GetValue(valName)?.ToString() ?? string.Empty;

                var entry = new StartupEntry
                {
                    Name = valName,
                    Command = cmd,
                    Location = location,
                    IsEnabled = isEnabled,
                    Publisher = ExtractPublisherOrFileName(cmd),
                    Impact = EstimateImpact(cmd),
                    RawLocationPath = $@"{hive.Name}\{subKeyPath}"
                };

                entries.Add(entry);
            }
        }
        catch
        {
            // Ignore access errors
        }
    }

    private static void ScanFolder(string folderPath, StartupLocation location, List<StartupEntry> entries)
    {
        try
        {
            if (!Directory.Exists(folderPath)) return;

            foreach (var file in Directory.GetFiles(folderPath))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                bool isEnabled = !file.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase);

                var entry = new StartupEntry
                {
                    Name = isEnabled ? Path.GetFileName(file) : Path.GetFileNameWithoutExtension(name),
                    Command = file,
                    Location = location,
                    IsEnabled = isEnabled,
                    Publisher = "Shortcut File",
                    Impact = StartupImpact.Medium,
                    RawLocationPath = file
                };

                entries.Add(entry);
            }
        }
        catch
        {
            // Ignore
        }
    }

    private static string ExtractPublisherOrFileName(string command)
    {
        try
        {
            string clean = command.Trim().Trim('"');
            int spaceIdx = clean.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (spaceIdx > 0)
            {
                clean = clean.Substring(0, spaceIdx + 4);
            }

            if (File.Exists(clean))
            {
                var vi = FileVersionInfo.GetVersionInfo(clean);
                if (!string.IsNullOrWhiteSpace(vi.CompanyName)) return vi.CompanyName;
                if (!string.IsNullOrWhiteSpace(vi.FileDescription)) return vi.FileDescription;
            }

            return Path.GetFileName(clean);
        }
        catch
        {
            return "Application";
        }
    }

    private static StartupImpact EstimateImpact(string command)
    {
        var lower = command.ToLowerInvariant();
        if (lower.Contains("update") || lower.Contains("helper") || lower.Contains("tray")) return StartupImpact.Low;
        if (lower.Contains("discord") || lower.Contains("spotify") || lower.Contains("steam") || lower.Contains("epic")) return StartupImpact.High;
        return StartupImpact.Medium;
    }

    private static void ScanScheduledTasks(List<StartupEntry> entries)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = "/query /fo CSV /nh",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var proc = Process.Start(psi);
            if (proc != null)
            {
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit(5000);

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { "\",\"" }, StringSplitOptions.None);
                    if (parts.Length >= 3)
                    {
                        string taskName = parts[0].Trim('"');
                        string status = parts[2].Trim('"');

                        // Filter out internal OS service tasks
                        if (taskName.StartsWith(@"\Microsoft\Windows\UpdateOrchestrator", StringComparison.OrdinalIgnoreCase) ||
                            taskName.StartsWith(@"\Microsoft\Windows\Servicing", StringComparison.OrdinalIgnoreCase) ||
                            taskName.StartsWith(@"\Microsoft\Windows\SystemRestore", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (taskName.Contains("Update", StringComparison.OrdinalIgnoreCase) ||
                            taskName.Contains("Telemetry", StringComparison.OrdinalIgnoreCase) ||
                            taskName.Contains("Launch", StringComparison.OrdinalIgnoreCase) ||
                            taskName.Contains("Startup", StringComparison.OrdinalIgnoreCase) ||
                            !taskName.StartsWith(@"\Microsoft\Windows\", StringComparison.OrdinalIgnoreCase))
                        {
                            bool isReady = !status.Equals("Disabled", StringComparison.OrdinalIgnoreCase);
                            string cleanName = taskName.TrimStart('\\');

                            entries.Add(new StartupEntry
                            {
                                Name = cleanName,
                                Command = $"schtasks /run /tn \"{taskName}\"",
                                Location = StartupLocation.TaskSchedulerLogon,
                                IsEnabled = isReady,
                                Publisher = cleanName.Contains('\\') ? cleanName.Split('\\')[0] : "Task Scheduler",
                                Impact = StartupImpact.Medium,
                                RawLocationPath = taskName
                            });
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore if schtasks is restricted
        }
    }
}
