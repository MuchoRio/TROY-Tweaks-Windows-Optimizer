using System.Diagnostics;
using System.Security.Principal;

namespace NRTX.Optimizer.Core.Safety;

public static class PrivilegeGuard
{
    public static bool IsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    public static bool RelaunchAsAdmin(string[]? args = null)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? "",
                UseShellExecute = true,
                Verb = "runas"
            };

            if (args != null && args.Length > 0)
            {
                processInfo.Arguments = string.Join(" ", args);
            }

            Process.Start(processInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
