using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Diagnostics;

namespace SPT_AKI_Installer.Aki.Helper
{
    public class PreCheckHelper
    {
        private const string registryInstall = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov";

        /// <summary>
        /// gets the original EFT game path
        /// </summary>
        /// <returns>Path or null</returns>
        public string DetectOriginalGamePath()
        {
            // We can't detect the installed path on non-Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return null;

            var uninstallStringValue = Registry.LocalMachine.OpenSubKey(registryInstall, false)
                ?.GetValue("UninstallString");
            var info = (uninstallStringValue is string key) ? new FileInfo(key) : null;
            return info?.DirectoryName;
        }

        /// <summary>
        /// checks path is not null, out = gamePath
        /// </summary>
        /// <param name="gamePath"></param>
        public void GameCheck(out string gamePath)
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

        /// <summary>
        /// Checks version of EFT installed, Then checks that matches the Zip, out = patch version number 0.12.12.*here*
        /// </summary>
        /// <param name="gamePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="patchRef"></param>
        /// <returns>bool</returns>
        public bool PatcherCheck(string gamePath,string targetPath, out string patchRef)
        {
            StringHelper stringHelper = new StringHelper();
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(Path.Join(gamePath + "/EscapeFromTarkov.exe"));

            string versionCheck = stringHelper.Splitter(version.ProductVersion, '-', '.', 2);
            LogHelper.Info($"GAME VERSION IS: {version.ProductVersion}");
            string patcherRef = FileHelper.FindFile(targetPath, versionCheck);

            if (patcherRef != null)
            {
                patchRef = patcherRef;
                return true;
            }
            patchRef = null;
            return false;
        }

        /// <summary>
        /// Checks Aki Zip is 2.3.1 currently
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="akiRef"></param>
        /// <returns>bool</returns>
        public bool AkiCheck(string targetPath,out string akiRef)
        {
            string aki = FileHelper.FindFile(targetPath, "2.3.1");

            if (aki != null)
            {
                akiRef = aki;
                return true;
            }
            akiRef = null;
            return false;
        }
    }
}
