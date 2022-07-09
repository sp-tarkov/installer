using System.Diagnostics;
using SPT_AKI_Installer.Aki.Core.Model;

namespace SPT_AKI_Installer.Aki.Helper
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

    public class ProcessHelper
    {
        public GenericResult PatchClientFiles(string exeDir, string workingDir)
        {
            var process = new Process();
            process.StartInfo.FileName = exeDir;
            process.StartInfo.WorkingDirectory = workingDir;
            process.EnableRaisingEvents = true;
            process.StartInfo.Arguments = "autoclose";
            process.Start();

            process.WaitForExit();

            switch ((PatcherExitCode)process.ExitCode)
            {
                case PatcherExitCode.Success:
                    return GenericResult.FromSuccess("Patcher Finished Successfully, extracting Aki");

                case PatcherExitCode.ProgramClosed:
                    return GenericResult.FromError("Patcher was closed before completing!");

                case PatcherExitCode.EftExeNotFound:
                    return GenericResult.FromError("EscapeFromTarkov.exe is missing from the install Path");

                case PatcherExitCode.NoPatchFolder:
                    return GenericResult.FromError("Patchers Folder called 'Aki_Patches' is missing");

                case PatcherExitCode.MissingFile:
                    return GenericResult.FromError("EFT files was missing a Vital file to continue");

                case PatcherExitCode.PatchFailed:
                    return GenericResult.FromError("A patch failed to apply");

                default:
                    return GenericResult.FromError("an unknown error occurred in the patcher");
            }
        }
    }
}
