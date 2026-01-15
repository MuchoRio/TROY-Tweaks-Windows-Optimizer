using Microsoft.Win32;

namespace NRTX.Optimizer.Core.Safety;

public static class SafeRegistry
{
    public static bool SetDword(RegistryHive hive, string subKey, string valueName, int value)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.CreateSubKey(subKey, true);
            if (key == null) return false;
            key.SetValue(valueName, value, RegistryValueKind.DWord);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool SetString(RegistryHive hive, string subKey, string valueName, string value)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.CreateSubKey(subKey, true);
            if (key == null) return false;
            key.SetValue(valueName, value, RegistryValueKind.String);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool SetBinary(RegistryHive hive, string subKey, string valueName, byte[] value)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.CreateSubKey(subKey, true);
            if (key == null) return false;
            key.SetValue(valueName, value, RegistryValueKind.Binary);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static int? GetDword(RegistryHive hive, string subKey, string valueName)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(subKey, false);
            if (key == null) return null;

            var val = key.GetValue(valueName);
            if (val is int intVal) return intVal;
            if (val != null && int.TryParse(val.ToString(), out var parsed)) return parsed;

            return null;
        }
        catch
        {
            return null;
        }
    }

    public static string? GetString(RegistryHive hive, string subKey, string valueName)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(subKey, false);
            if (key == null) return null;

            return key.GetValue(valueName)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    public static byte[]? GetBinary(RegistryHive hive, string subKey, string valueName)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(subKey, false);
            if (key == null) return null;

            return key.GetValue(valueName) as byte[];
        }
        catch
        {
            return null;
        }
    }

    public static bool DeleteValue(RegistryHive hive, string subKey, string valueName)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(subKey, true);
            if (key == null) return true; // Already gone
            if (key.GetValue(valueName) != null)
            {
                key.DeleteValue(valueName);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool DeleteSubKeyTree(RegistryHive hive, string subKey)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            baseKey.DeleteSubKeyTree(subKey, false);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
