using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Diagnostics;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class PreCheckHelper
    {
        private const string registryInstall = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov";
        private static string OGGamePath;
        public static string gameVersion;
        private static string patchZip;
        private static string akiZip;

        public static string DetectOriginalGamePath()
        {
            // We can't detect the installed path on non-Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return null;

            var uninstallStringValue = Registry.LocalMachine.OpenSubKey(registryInstall, false)
                ?.GetValue("UninstallString");
            var info = (uninstallStringValue is string key) ? new FileInfo(key) : null;
            OGGamePath = info?.DirectoryName;

            return OGGamePath;
        }

        public static void GameCheck(out string gamePath)
        {
            string Path = DetectOriginalGamePath();

            if (Path == null)
            {
                LogHelper.Error("EFT IS NOT INSTALLED!");
                LogHelper.Error("Press enter to close the app");
                Console.ReadKey();
                Environment.Exit(0);
            }
            gamePath = Path;
        }

        public static void DetectOriginalGameVersion(string gamePath)
        {
            gameVersion = FileVersionInfo.GetVersionInfo(Path.Join(gamePath + "/EscapeFromTarkov.exe")).ProductVersion.Replace('-', '.').Split('.')[^2];
        }

        public static void PatcherZipCheck(string gamePath, string targetPath, out string patcherZipPath)
        {
            // example patch name - Patcher.12.12.15.17861.to.12.12.15.17349.zip
            patchZip = FileHelper.FindFile(targetPath, gameVersion, "Patcher");
            if (patchZip == null)
            {
                patchZip = FileHelper.FindFile(targetPath, "PATCHER");
            }
            patcherZipPath = patchZip;
        }

        public static void AkiZipCheck(string targetPath, out string akiZipPath)
        {
            // example aki name - RELEASE-SPT-2.3.1-17349.zip
            akiZip = FileHelper.FindFile(targetPath, "SPT", "RELEASE");
            if (akiZip == null)
            {
                akiZip = FileHelper.FindFile(targetPath, "AKI");
            }
            akiZipPath = akiZip;
        }
    }
}
