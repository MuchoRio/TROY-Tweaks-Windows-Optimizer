using System.Text.Json;
using Microsoft.Win32;

namespace NRTX.Optimizer.Core.Safety;

public class RegistryStateEntry
{
    public string Hive { get; set; } = string.Empty;
    public string SubKey { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public string? ValueData { get; set; }
    public string ValueKind { get; set; } = "String";
    public bool ExistedBefore { get; set; }
}

public class SystemSnapshot
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> AppliedTweakIds { get; set; } = [];
    public List<RegistryStateEntry> RegistryStates { get; set; } = [];
}

public static class SnapshotManager
{
    private static readonly string SnapshotDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NRTX_Optimizer",
        "Snapshots"
    );

    public static string EnsureSnapshotDirectory()
    {
        if (!Directory.Exists(SnapshotDir))
        {
            Directory.CreateDirectory(SnapshotDir);
        }
        return SnapshotDir;
    }

    public static RegistryStateEntry CaptureValueState(RegistryHive hive, string subKey, string valueName)
    {
        var entry = new RegistryStateEntry
        {
            Hive = hive.ToString(),
            SubKey = subKey,
            ValueName = valueName,
            ExistedBefore = false
        };

        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(subKey, false);
            if (key != null)
            {
                var val = key.GetValue(valueName);
                if (val != null)
                {
                    entry.ExistedBefore = true;
                    var kind = key.GetValueKind(valueName);
                    entry.ValueKind = kind.ToString();

                    if (val is byte[] bytes)
                    {
                        entry.ValueData = Convert.ToBase64String(bytes);
                    }
                    else
                    {
                        entry.ValueData = val.ToString();
                    }
                }
            }
        }
        catch { }

        return entry;
    }

    public static string SaveSnapshot(SystemSnapshot snapshot)
    {
        EnsureSnapshotDirectory();
        if (string.IsNullOrWhiteSpace(snapshot.Id))
        {
            snapshot.Id = $"snap_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        }

        var filePath = Path.Combine(SnapshotDir, $"{snapshot.Id}.json");
        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);

        AuditLogger.Log(AuditLogLevel.Info, "SnapshotManager", $"Created system snapshot {snapshot.Id} ({snapshot.RegistryStates.Count} states recorded)");
        return filePath;
    }

    public static List<SystemSnapshot> ListSnapshots()
    {
        EnsureSnapshotDirectory();
        var list = new List<SystemSnapshot>();

        try
        {
            foreach (var file in Directory.GetFiles(SnapshotDir, "snap_*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var snap = JsonSerializer.Deserialize<SystemSnapshot>(json);
                    if (snap != null) list.Add(snap);
                }
                catch { }
            }
        }
        catch { }

        return list.OrderByDescending(s => s.CreatedAt).ToList();
    }

    public static bool RestoreSnapshot(string snapshotId)
    {
        EnsureSnapshotDirectory();
        var filePath = Path.Combine(SnapshotDir, snapshotId.EndsWith(".json") ? snapshotId : $"{snapshotId}.json");
        if (!File.Exists(filePath)) return false;

        try
        {
            var json = File.ReadAllText(filePath);
            var snapshot = JsonSerializer.Deserialize<SystemSnapshot>(json);
            if (snapshot == null) return false;

            foreach (var entry in snapshot.RegistryStates)
            {
                if (!Enum.TryParse<RegistryHive>(entry.Hive, out var hive)) continue;

                if (!entry.ExistedBefore)
                {
                    SafeRegistry.DeleteValue(hive, entry.SubKey, entry.ValueName);
                }
                else if (entry.ValueData != null)
                {
                    if (entry.ValueKind.Equals("Binary", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var bytes = Convert.FromBase64String(entry.ValueData);
                            SafeRegistry.SetBinary(hive, entry.SubKey, entry.ValueName, bytes);
                        }
                        catch
                        {
                            // In case legacy corrupted snapshot had non-base64 data
                        }
                    }
                    else if (entry.ValueKind.Equals("DWord", StringComparison.OrdinalIgnoreCase) && int.TryParse(entry.ValueData, out var intVal))
                    {
                        SafeRegistry.SetDword(hive, entry.SubKey, entry.ValueName, intVal);
                    }
                    else
                    {
                        SafeRegistry.SetString(hive, entry.SubKey, entry.ValueName, entry.ValueData);
                    }
                }
            }

            AuditLogger.Log(AuditLogLevel.Success, "SnapshotManager", $"Restored system state from snapshot {snapshotId}");
            return true;
        }
        catch (Exception ex)
        {
            AuditLogger.Log(AuditLogLevel.Error, "SnapshotManager", $"Failed to restore snapshot {snapshotId}: {ex.Message}");
            return false;
        }
    }
}
