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
    // add figlet for SPT-AKI INSTALLER
    // locales, language selection
    // PreCheckHelper.AkiCheck is currently hardcoded for 2.3.1
    // get waffle to add exit code on patcher
    // remove all user input other than errors

    //comments:
    // static: FileHelper, ZipHelper, LogHelper 
    // nonStatic: ProcessHelper, PreCheckHelper, StringHelper

    public static class SPTinstaller
    {
        static void Main(string[] args)
        {
            string targetPath = Environment.CurrentDirectory;
            PreCheckHelper preCheckHelper = new();
#if DEBUG
            targetPath = @"D:\install";
#endif
            preCheckHelper.GameCheck(out string gamePath);

            if (preCheckHelper.PatcherCheck(gamePath,targetPath, out string patchRef))
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

            if (preCheckHelper.AkiCheck(targetPath,out string akiRef))
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

            GameCopy(gamePath, targetPath, akiRef);
        }

        /// <summary>
        /// copies and pastes EFT to AKI installer test folder
        /// </summary>
        /// <param name="gamePath"></param>
        /// <param name="targetPath"></param>
        static void GameCopy(string gamePath, string targetPath, string akiRef)
        {
#if !DEBUG
            FileHelper.CopyDirectory(gamePath, targetPath, true);
#endif
            LogHelper.User("GAME HAS BEEN COPIED, PRESS ENTER TO EXTRACT PATCHER!");
            Console.ReadKey();

            PatcherCopy(gamePath, targetPath, akiRef);
        }

        /// <summary>
        /// extracts patcher and moves out inner folders
        /// </summary>
        /// <param name="patchRef"></param>
        /// <param name="targetPath"></param>
        /// <param name="akiRef"></param>
        static void PatcherCopy(string patchRef, string targetPath, string akiRef)
        {
#if !DEBUG
            ZipHelper.Decompress(patchRef, targetPath);
            FileHelper.FindFolder(patchRef, targetPath, out DirectoryInfo dir);
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
#endif
            PatcherProcessStart(targetPath, akiRef);
        }

        /// <summary>
        /// starts patcher and checks for user input to exit patcher and proceed
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="akiRef"></param>
        static void PatcherProcessStart(string targetPath, string akiRef)
        {
#if !DEBUG
            LogHelper.Info("PATCHER HAS BEEN EXTRACTED, STARTING PATCHER!");
            ProcessHelper patcherProcess = new();
            patcherProcess.StartProcess(Path.Join(targetPath + "/patcher.exe"), targetPath);
#endif
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
#if !DEBUG
                patcherProcess.EndProcess();
                Thread.Sleep(1000);
                FileHelper.DeleteFile("file", targetPath + "/patcher.exe");
                ZipHelper.Decompress(akiRef, targetPath);
#endif
                LogHelper.Info("AKI HAS BEEN EXTRACTED, RUN THE SERVER AND WAIT TILL YOU SEE HAPPY SERVER THEN LAUNCHER AND ENJOY!");
                LogHelper.User("PRESS ENTER TO CLOSE THE APP");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}