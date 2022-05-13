using System.IO;
using System;
using System.Diagnostics;
using System.Threading;
using Installer.Aki.Helper;

namespace Installer.Aki.Core
{
    //TODO:
    // delete patcher zip and aki zip

    public static class SPTinstaller
    {
        private static string patchRef;
        private static string akiRef;
        private static DirectoryInfo dir;

        static void Main(string[] args)
        {
            string gamePath = GameHelper.DetectOriginalGamePath();

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
            FileHelper.CopyDirectory(gamePath, Environment.CurrentDirectory, true);
            LogHelper.User("GAME HAS BEEN COPIED, PRESS ENTER TO EXTRACT PATCHER!");
            Console.ReadKey();

            // extracts patcher and moves out inner folders
            ZipHelper.Decompress(patchRef, Environment.CurrentDirectory);
            FileHelper.FindFolder(patchRef, out dir);
            FileHelper.CopyDirectory(dir.FullName, Environment.CurrentDirectory, true);
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
            LogHelper.User("PATCHER HAS BEEN EXTRACTED, STARTING PATCHER!");
            ProcessHelper patcherProcess = new ProcessHelper();
            patcherProcess.StartProcess(Path.Join(Environment.CurrentDirectory + "/patcher.exe"), Environment.CurrentDirectory);
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
                FileHelper.DeleteFile("file", Environment.CurrentDirectory + "/patcher.exe");
                ZipHelper.Decompress(akiRef, Environment.CurrentDirectory);
                LogHelper.Info("AKI HAS BEEN EXTRACTED, RUN THE SERVER AND WAIT TILL YOU SEE HAPPY SERVER THEN LAUNCHER AND ENJOY!");
                LogHelper.User("PRESS ENTER TO CLOSE THE APP");
                Console.ReadKey();
            }
        }

        private static bool PatcherCheck(string gamePath, out string patchRef)
        {
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(Path.Join(gamePath + "/EscapeFromTarkov.exe"));
            string versionCheck = StringHelper.Splitter(version.ProductVersion, '-', '.', 2);
            LogHelper.Info($"GAME VERSION IS: {version.ProductVersion}");
            string patcherRef = FileHelper.FindFile(Environment.CurrentDirectory, versionCheck);

            if (patcherRef != null)
            {
                patchRef = patcherRef;
                return true;
            }
            patchRef = null;
            return false;
        }

        private static bool AkiCheck(out string akiRef)
        {
            string aki = FileHelper.FindFile(Environment.CurrentDirectory, "2.3.1");

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