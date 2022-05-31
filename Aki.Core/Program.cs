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
                CloseApp("Unable to find EFT OG directory! \n please make sure EFT is installed! \n please also run EFT once!");
            }

            if (originalGamePath == targetPath)
            {
                CloseApp("Installer is located in EFT's original directory! \n Please move the installer to a seperate folder as per the guide!");
            }

            var checkForExistingFiles = FileHelper.FindFile(targetPath, "EscapeFromTarkov.exe");
            if (checkForExistingFiles != null)
            {
                CloseApp("Installer is located in a Folder that has existing Game Files \n Please make sure the installer is in a fresh folder as per the guide");
            }

            LogHelper.User("We need to download files during this installation.");
            LogHelper.User("Are you ok with this? type yes or no");
            var userReponse = Console.ReadLine();

            while (!string.Equals(userReponse, "yes", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(userReponse, "no", StringComparison.OrdinalIgnoreCase))
            {
                LogHelper.Warning("Response was not yes or no, please respond with yes or no");
                userReponse = Console.ReadLine();
            }

            if (string.Equals(userReponse, "no", StringComparison.OrdinalIgnoreCase))
            {
                CloseApp("you selected no, we need to download this to continue with auto installation \n Press enter to close the app");
            }

            LogHelper.Info("Downloading ClientVersions.json...");
            var jsonDownload = DownloadHelper.DownloadFileAsync(targetPath, "https://dev.sp-tarkov.com/api/v1/repos/CWX/Installer_Test/raw/ClientVersions.json", "/ClientVersions.json");
            while (jsonDownload.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
            {
            }
            LogHelper.Info("Downloading Complete, Checking Versions!");
            DownloadHelper.ReadJson(targetPath);

            LogHelper.Info($"Original game path detected, Game version: { DownloadHelper.ogClient } Detected");
            LogHelper.Info($"TargetClient version for this Game version is: { DownloadHelper.targetClient }");
            if (string.Equals(DownloadHelper.patchNeedCheck, "true", StringComparison.OrdinalIgnoreCase))
            {
                LogHelper.Info("Patching IS required!");
            }

            if (string.Equals(DownloadHelper.patchNeedCheck, "false", StringComparison.OrdinalIgnoreCase))
            {
                LogHelper.Info("Patching is not required!");
            }

            LogHelper.Info($"TargetAki version for this GameVersion is { DownloadHelper.targetAki }");
            LogHelper.Info("Checking if Zips already exist in directory");

            PreCheckHelper.PatcherZipCheck(originalGamePath, targetPath, out string patcherZipPath);
            PreCheckHelper.AkiZipCheck(targetPath, out string akiZipPath);

            if (patcherZipPath == null && string.Equals(DownloadHelper.patchNeedCheck, "true", StringComparison.OrdinalIgnoreCase))
            {
                LogHelper.Info("Unable to find Patcher Zip in Directory");
                LogHelper.Info("Downloading Patcher Zip now!");
                var task = DownloadHelper.DownloadFileAsync(targetPath, DownloadHelper.patcherLink, "/PATCHERZIP.zip");
                while(task.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
                {
                }
                LogHelper.Info("Download Complete!");
            }
            else if (string.Equals(DownloadHelper.patchNeedCheck, "false", StringComparison.OrdinalIgnoreCase))
            {
                LogHelper.Info("Did not check for Patcher as its not needed");
            }

            if (akiZipPath == null)
            {
                LogHelper.Info("Unable to find Aki Zip in Directory");
                LogHelper.Info("Downloading Aki Zip now!");
                var task = DownloadHelper.DownloadFileAsync(targetPath, DownloadHelper.akiLink, "/AKIZIP.zip");
                while (task.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
                {
                }
                LogHelper.Info("Download Complete!");
            }
            LogHelper.Info("Ready to continue with installation");

            PreCheckHelper.PatcherZipCheck(originalGamePath, targetPath, out patcherZipPath);
            PreCheckHelper.AkiZipCheck(targetPath, out akiZipPath);

            LogHelper.Info("Copying game files");

            GameCopy(originalGamePath, targetPath);
            if (string.Equals(DownloadHelper.patchNeedCheck, "true", StringComparison.OrdinalIgnoreCase))
            {
                PatcherCopy(targetPath, patcherZipPath);
                PatcherProcess(targetPath);
            }

            AkiInstall(targetPath, akiZipPath);
            DeleteZip(patcherZipPath, akiZipPath, Path.Join(targetPath, "/ClientVersions.json"));
        }

        static void GameCopy(string originalGamePath, string targetPath)
        {
            FileHelper.CopyDirectory(originalGamePath, targetPath, true);
            LogHelper.Info("Game has been copied, Extracting patcher");
        }

        static void PatcherCopy(string targetPath, string patcherZipPath)
        {
            ZipHelper.Decompress(patcherZipPath, targetPath);
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
            LogHelper.Info("patcher has been extracted, starting patcher");
            ProcessHelper patcherProcess = new();
            patcherProcess.StartProcess(Path.Join(targetPath + "/patcher.exe"), targetPath);

            FileHelper.DeleteFiles(Path.Join(targetPath, "/patcher.exe"));
        }

        static void AkiInstall(string targetPath, string akiZipPath)
        {
            ZipHelper.Decompress(akiZipPath, targetPath);
            LogHelper.Info("Aki has been extracted");
        }

        static void DeleteZip(string patcherZipPath, string akiZipPath, string versionJson)
        {
            FileHelper.DeleteFiles(patcherZipPath, false);
            FileHelper.DeleteFiles(akiZipPath, false);
            FileHelper.DeleteFiles(versionJson, false);

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