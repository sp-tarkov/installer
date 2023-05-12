using SPTInstaller.Models;
using System.Diagnostics;
using System.IO;

namespace SPTInstaller.Aki.Helper
{
    public enum PatcherExitCode
    {
        ProgramClosed = 0,
        Success = 10,
        EftExeNotFound = 11,
        NoPatchFolder = 12,
        MissingFile = 13,
        MissingDir = 14,
        PatchFailed = 15
    }

    public static class ProcessHelper
    {
        public static Result PatchClientFiles(FileInfo executable, DirectoryInfo workingDir)
        {
            if (!executable.Exists || !workingDir.Exists)
            {
                return Result.FromError($"Could not find executable ({executable.Name}) or working directory ({workingDir.Name})");
            }

            var process = new Process();
            process.StartInfo.FileName = executable.FullName;
            process.StartInfo.WorkingDirectory = workingDir.FullName;
            process.EnableRaisingEvents = true;
            process.StartInfo.Arguments = "autoclose";
            process.Start();

            process.WaitForExit();

            switch ((PatcherExitCode)process.ExitCode)
            {
                case PatcherExitCode.Success:
                    return Result.FromSuccess("Patcher Finished Successfully, extracting Aki");

                case PatcherExitCode.ProgramClosed:
                    return Result.FromError("Patcher was closed before completing!");

                case PatcherExitCode.EftExeNotFound:
                    return Result.FromError("EscapeFromTarkov.exe is missing from the install Path");

                case PatcherExitCode.NoPatchFolder:
                    return Result.FromError("Patchers Folder called 'Aki_Patches' is missing");

                case PatcherExitCode.MissingFile:
                    return Result.FromError("EFT files was missing a Vital file to continue");

                case PatcherExitCode.PatchFailed:
                    return Result.FromError("A patch failed to apply");

                default:
                    return Result.FromError("an unknown error occurred in the patcher");
            }
        }
    }
}
