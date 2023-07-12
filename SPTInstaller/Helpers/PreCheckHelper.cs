using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using SPTInstaller.Models;

namespace SPTInstaller.Helpers;

public static class PreCheckHelper
{
    private const string registryInstall = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov";

    public static string DetectOriginalGamePath()
    {
        // We can't detect the installed path on non-Windows
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        var uninstallStringValue = Registry.LocalMachine.OpenSubKey(registryInstall, false)
            ?.GetValue("UninstallString");
        var info = (uninstallStringValue is string key) ? new FileInfo(key) : null;

        return info?.DirectoryName;
    }

    public static Result DetectOriginalGameVersion(string gamePath)
    {
        try
        {
            string version = FileVersionInfo.GetVersionInfo(Path.Join(gamePath + "/EscapeFromTarkov.exe")).ProductVersion.Replace('-', '.').Split('.')[^2];
            return Result.FromSuccess(version);
        }
        catch (Exception ex)
        {
            return Result.FromError($"File not found: {ex.Message}");
        }
    }
}