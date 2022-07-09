using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace SPT_AKI_Installer.Aki.Helper
{
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

        public static string DetectOriginalGameVersion(string gamePath)
        {
            return FileVersionInfo.GetVersionInfo(Path.Join(gamePath + "/EscapeFromTarkov.exe")).ProductVersion.Replace('-', '.').Split('.')[^2];
        }

        //public static string GetPatcherZipPath(string gameVersion, string targetPath)
        //{
        //    // example patch name - Patcher.12.12.15.17861.to.12.12.15.17349.zip
        //    var patchZip = FileHelper.FindFile(targetPath, gameVersion, "Patcher");
        //    if (patchZip == null)
        //    {
        //        patchZip = FileHelper.FindFile(targetPath, "PATCHERZIP");
        //    }
            
        //    return patchZip;
        //}

        //public static string GetAkiZipPath(string targetPath)
        //{
        //    // example aki name - RELEASE-SPT-2.3.1-17349.zip
        //    var akiZip = FileHelper.FindFile(targetPath, "SPT", "RELEASE");

        //    if (akiZip == null)
        //    {
        //        akiZip = FileHelper.FindFile(targetPath, "AKIZIP");
        //    }

        //    return akiZip;
        //}
    }
}
