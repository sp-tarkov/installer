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
        private static string gameVersion;
        private static string patchZip;
        private static string patchToVersion;
        private static string akiZip;
        private static string akiVersion;

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

        public static void PatcherZipCheck(string gamePath, string targetPath, out string patcherZipPath)
        {
            // example patch name - Patcher.12.12.15.17861.to.12.12.15.17349.zip
            gameVersion = FileVersionInfo.GetVersionInfo(Path.Join(gamePath + "/EscapeFromTarkov.exe")).ProductVersion.Replace('-', '.').Split('.')[^2];
            patchZip = FileHelper.FindFile(targetPath, gameVersion, "Patcher");
            patchToVersion = patchZip?.Split('.')[^2];
            patcherZipPath = patchZip;
        }

        public static void AkiZipCheck(string targetPath, out string akiZipPath)
        {
            // example aki name - RELEASE-SPT-2.3.1-17349.zip
            akiZip = FileHelper.FindFile(targetPath, "SPT", "RELEASE");
            akiVersion = akiZip?.Replace('-', '.').Split('.')[^2];
            akiZipPath = akiZip;
        }

        /// <summary>
        /// will return true if Patcher version at the end of the zip matches aki zip version
        /// </summary>
        /// <returns>bool</returns>
        public static bool PatcherAkiCheck()
        {
            return patchToVersion == akiVersion;
        }

        /// <summary>
        /// will return true if game version is not equal to aki zip version (patcher is needed)
        /// </summary>
        /// <returns>bool</returns>
        public static bool PatcherNeededCheck()
        {
            return gameVersion != akiVersion;
        }
    }
}
