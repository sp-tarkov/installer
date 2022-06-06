using Spectre.Console;
using SPT_AKI_Installer.Aki.Helper;
using System;
using System.IO;
using System.Threading;

namespace SPT_AKI_Installer.Aki.Core
{
    //TODO:
    // locales, language selection

    public static class SPTinstaller
    {
        static void Main()
        {
            string targetPath = Environment.CurrentDirectory;
#if DEBUG
            targetPath = @"D:\install";
#endif
            SpectreHelper.Figlet("SPT-AKI INSTALLER", Color.Yellow);
            PreCheckHelper.GameCheck(out string originalGamePath);
            PreCheckHelper.DetectOriginalGameVersion(originalGamePath);

            if (originalGamePath == null)
            {
                CloseApp("Unable to find EFT OG directory \n please make sure EFT is installed \n please also run EFT once");
            }

            if (originalGamePath == targetPath)
            {
                CloseApp("Installer is located in EFT's original directory \n Please move the installer to a seperate folder as per the guide");
            }

            var checkForExistingFiles = FileHelper.FindFile(targetPath, "EscapeFromTarkov.exe");
            if (checkForExistingFiles != null)
            {
                CloseApp("Installer is located in a Folder that has existing Game Files \n Please make sure the installer is in a fresh folder as per the guide");
            }

            LogHelper.Info($"Your current game version: { PreCheckHelper.gameVersion }");

            LogHelper.Info("Checking releases for AKI and the Patcher");

            var check = DownloadHelper.ReleaseCheck();

            while (check.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
            {
            }

            LogHelper.Info("Checking if Zips already exist in directory");

            PreCheckHelper.PatcherZipCheck(originalGamePath, targetPath, out string patcherZipPath);
            PreCheckHelper.AkiZipCheck(targetPath, out string akiZipPath);

            if (patcherZipPath == null && DownloadHelper.patchNeedCheck)
            {
                LogHelper.Info("No Patcher zip file present in directory, downloading...");
                var task = DownloadHelper.DownloadFileAsync(targetPath, DownloadHelper.patcherLink, "/PATCHERZIP.zip");
                while(task.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
                {
                }
                LogHelper.Info("Download Complete!");
            }

            if (akiZipPath == null)
            {
                LogHelper.Info("No AKI zip file present in directory, downloading...");
                var task = DownloadHelper.DownloadFileAsync(targetPath, DownloadHelper.akiLink, "/AKIZIP.zip");
                while (task.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
                {
                }
                LogHelper.Info("Download Complete!");
            }

            PreCheckHelper.PatcherZipCheck(originalGamePath, targetPath, out patcherZipPath);
            PreCheckHelper.AkiZipCheck(targetPath, out akiZipPath);

            LogHelper.Info("Copying game files");

            GameCopy(originalGamePath, targetPath);

            if (DownloadHelper.patchNeedCheck)
            {
                PatcherCopy(targetPath, patcherZipPath);
                PatcherProcess(targetPath);
            }

            AkiInstall(targetPath, akiZipPath);
            DeleteZip(patcherZipPath, akiZipPath);
        }

        static void GameCopy(string originalGamePath, string targetPath)
        {
            FileHelper.CopyDirectory(originalGamePath, targetPath, true);
            LogHelper.Info("Extracting patcher");
        }

        static void PatcherCopy(string targetPath, string patcherZipPath)
        {
            ZipHelper.ZipDecompress(patcherZipPath, targetPath);
            FileHelper.FindFolder(patcherZipPath, targetPath, out DirectoryInfo dir);
            FileHelper.CopyDirectory(dir.FullName, targetPath, true);

            if (dir.Exists)
            {
                dir.Delete(true);
                dir.Refresh();
                if (dir.Exists)
                {
                    LogHelper.Error("unable to delete patcher folder");
                    LogHelper.Error($"please delete folder called {dir.FullName}");
                }
            }
        }

        static void PatcherProcess(string targetPath)
        {
            LogHelper.Info("Starting patcher");
            ProcessHelper patcherProcess = new();
            patcherProcess.StartProcess(Path.Join(targetPath + "/patcher.exe"), targetPath);

            FileHelper.DeleteFiles(Path.Join(targetPath, "/patcher.exe"));
        }

        static void AkiInstall(string targetPath, string akiZipPath)
        {
            ZipHelper.ZipDecompress(akiZipPath, targetPath);
            LogHelper.Info("Aki has been extracted");
        }

        static void DeleteZip(string patcherZipPath, string akiZipPath)
        {
            FileHelper.DeleteFiles(patcherZipPath, false);
            FileHelper.DeleteFiles(akiZipPath, false);

            LogHelper.User("Removed Zips, Press enter to close the installer, you can then delete the installer");
            LogHelper.User("ENJOY SPT-AKI!");
            Console.ReadKey();
            Environment.Exit(0);
        }

        static void CloseApp(string text)
        {
            LogHelper.Warning(text);
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}