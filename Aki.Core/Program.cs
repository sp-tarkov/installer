using System.IO;
using System;
using System.Diagnostics;
using System.Threading;
using SPT_AKI_Installer.Aki.Helper;
using Spectre.Console;

namespace SPT_AKI_Installer.Aki.Core
{
    //TODO:
    // delete patcher zip and aki zip
    // progress for copyDirectory
    // move Game/patcher/aki check methods out of core
    // add figlet for SPT-AKI INSTALLER
    // locales, language selection

    public static class SPTinstaller
    {
        public static string targetPath = Environment.CurrentDirectory;
        private static string patchRef;
        private static string akiRef;
        private static DirectoryInfo dir;
        private static string gamePath;

        static void Main(string[] args)
        {
#if DEBUG
            targetPath = @"D:\install";
#endif
            GameCheck(out gamePath);

            if (PatcherCheck(gamePath, out patchRef))
            {
                LogHelper.Info($"Correct Zip for Patcher Present: {patchRef}");
            }
            else
            {
                LogHelper.Error("Patcher zip is Incorrect or missing");
                LogHelper.Error("Press enter to close the app");
                Console.ReadKey();
                Environment.Exit(0);
            }

            if (AkiCheck(out akiRef))
            {
                LogHelper.Info($"Correct Zip for SPT-AKI Present: {akiRef}");
            }
            else
            {
                LogHelper.Error("SPT-AKI zip is Incorrect or missing");
                LogHelper.Error("Press enter to close the app");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // checks for input to copy game files
            LogHelper.User("PLEASE PRESS ENTER TO COPY GAME FILES!");
            Console.ReadKey();

            // copies and pastes EFT to AKI installer test folder
#if !DEBUG
            FileHelper.CopyDirectory(gamePath, targetPath, true);
            LogHelper.User("GAME HAS BEEN COPIED, PRESS ENTER TO EXTRACT PATCHER!");
            Console.ReadKey();
#endif

            // extracts patcher and moves out inner folders
            ZipHelper.Decompress(patchRef, targetPath);
            FileHelper.FindFolder(patchRef, out dir);
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

            // starts patcher and checks for user input to exit patcher and proceed
            LogHelper.Info("PATCHER HAS BEEN EXTRACTED, STARTING PATCHER!");
            ProcessHelper patcherProcess = new();
            patcherProcess.StartProcess(Path.Join(targetPath + "/patcher.exe"), targetPath);
            LogHelper.User("PATCHER HAS BEEN STARTED, TYPE YES ONCE THE PATCHER IS COMPLETE!");
            var complete = Console.ReadLine();

            // waiting for user to enter "yes", if something else is entered do while loop
            while (!string.Equals(complete, "yes", StringComparison.OrdinalIgnoreCase))
            {
                LogHelper.Warning("YOU DIDNT TYPE YES, IF SOMETHING WENT WRONG MAKE A SUPPORT THREAD AND CLOSE THIS APP");
                LogHelper.User("IF IT DID FINISH TYPE YES NOW");
                complete = Console.ReadLine();
            }

            // if user input "yes" kill patcher process, delete patcher.exe, extract aki zip
            if (string.Equals(complete, "yes", StringComparison.OrdinalIgnoreCase))
            {
                patcherProcess.EndProcess();
                Thread.Sleep(1000);
                FileHelper.DeleteFile("file", targetPath + "/patcher.exe");
                ZipHelper.Decompress(akiRef, targetPath);
                LogHelper.Info("AKI HAS BEEN EXTRACTED, RUN THE SERVER AND WAIT TILL YOU SEE HAPPY SERVER THEN LAUNCHER AND ENJOY!");
                LogHelper.User("PRESS ENTER TO CLOSE THE APP");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// checks the game is installed, out = game directory
        /// </summary>
        /// <param name="gamePath"></param>
        private static void GameCheck(out string gamePath)
        {
            string Path = GameHelper.DetectOriginalGamePath();

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
        /// checks for patcher zip path, out = patcher path
        /// </summary>
        /// <param name="gamePath"></param>
        /// <param name="patchRef"></param>
        /// <returns>bool</returns>
        private static bool PatcherCheck(string gamePath, out string patchRef)
        {
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(Path.Join(gamePath + "/EscapeFromTarkov.exe"));
            string versionCheck = StringHelper.Splitter(version.ProductVersion, '-', '.', 2);
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
        /// checks for aki zip path, out = aki path
        /// </summary>
        /// <param name="akiRef"></param>
        /// <returns>bool</returns>
        private static bool AkiCheck(out string akiRef)
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