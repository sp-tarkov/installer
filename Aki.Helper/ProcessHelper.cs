using System.Diagnostics;
using System.Threading;
using System;

namespace SPT_AKI_Installer.Aki.Helper
{
    public class ProcessHelper
    {
        private Process _process;
        private string _exeDir;
        private string _workingDir;

        public void StartProcess(string exeDir, string workingDir)
        {
            _exeDir = exeDir;
            _workingDir = workingDir;
            _process = new Process();
            _process.StartInfo.FileName = exeDir;
            _process.StartInfo.WorkingDirectory = workingDir;
            _process.EnableRaisingEvents = true;
            _process.StartInfo.Arguments = "autoclose";
            _process.Start();

            _process.WaitForExit();
            ExitCodeCheck(_process.ExitCode);
        }

        public void ExitCodeCheck(int exitCode)
        {
            /*
            public enum PatcherExitCode
            {
                ProgramClosed = 0,
                Success = 10,
                EftExeNotFound = 11,
                NoPatchFolder = 12,
                MissingFile = 13,
                MissingDir = 14
            }
            */

            switch (exitCode)
            {
                case 0:
                    LogHelper.Warning("Patcher was closed before completing!");
                    LogHelper.Warning("If you need to start the patcher again, type RETRY");
                    LogHelper.Warning("If you want to close the installer, type CLOSE");
                    var response = Console.ReadLine();

                    while (!string.Equals(response, "retry", StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(response, "close", StringComparison.OrdinalIgnoreCase))
                    {
                        LogHelper.Warning("answer needs to be retry or close");
                        LogHelper.Warning("Try Again!");
                    }
                    if (string.Equals(response, "retry", StringComparison.OrdinalIgnoreCase))
                    {
                        StartProcess(_exeDir, _workingDir);
                        break;
                    }
                    if (string.Equals(response, "close", StringComparison.OrdinalIgnoreCase))
                    {
                        Environment.Exit(0);
                        break;
                    }
                    break;

                case 10:
                    LogHelper.User("Patcher Finished Successfully, extracting Aki");
                    break;

                case 11:
                    LogHelper.Error("EscapeFromTarkov.exe is missing from the install Path");
                    LogHelper.Warning("Check your game files in their original location are complete!");
                    LogHelper.Warning("Closing the installer in 20 seconds");
                    Thread.Sleep(20000);
                    Environment.Exit(0);
                    break;

                case 12:
                    LogHelper.Error("Patchers Folder called 'Aki_Patches' missing");
                    LogHelper.Warning("Closing the installer in 20 seconds");
                    Thread.Sleep(20000);
                    Environment.Exit(0);
                    break;

                case 13:
                    LogHelper.Error("EFT files was missing a Vital file to continue");
                    LogHelper.Warning("please reinstall EFT through the BSG launcher");
                    LogHelper.Warning("Closing the installer in 20 seconds");
                    Thread.Sleep(20000);
                    Environment.Exit(0);
                    break;

                case 14:
                    LogHelper.Error("Patcher Reported Missing Folder");
                    // check with Waffle what this one is
                    LogHelper.Warning("Closing the installer in 20 seconds");
                    Thread.Sleep(20000);
                    Environment.Exit(0);
                    break;
            }
        }
    }
}
