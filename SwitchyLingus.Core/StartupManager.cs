using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace SwitchyLingus.Core;

[SupportedOSPlatform("windows")]
public static class StartupManager
{
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "SwitchyLingus";

    public static bool IsRunOnStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        return key?.GetValue(AppName) != null;
    }

    public static void SetRunOnStartup(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
        if (key == null) return;

        if (enabled)
        {
            var exePath = Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly().Location;
            key.SetValue(AppName, "\"" + exePath + "\"");
        }
        else
        {
            key.DeleteValue(AppName, false);
        }
    }
}