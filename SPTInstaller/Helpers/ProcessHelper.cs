using System.Diagnostics;
using System.Text;
using System.Threading;
using SPTInstaller.Models;

namespace SPTInstaller.Helpers;

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
            return Result.FromError(
                $"Could not find executable ({executable.Name}) or working directory ({workingDir.Name})");
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
                return Result.FromSuccess("Patcher Finished Successfully, extracting SPT");
            
            case PatcherExitCode.ProgramClosed:
                return Result.FromError("Patcher was closed before completing!");
            
            case PatcherExitCode.EftExeNotFound:
                return Result.FromError("EscapeFromTarkov.exe is missing from the install Path");
            
            case PatcherExitCode.NoPatchFolder:
                return Result.FromError("Patchers Folder called 'SPT_Patches' is missing");
            
            case PatcherExitCode.MissingFile:
                return Result.FromError("EFT files was missing a Vital file to continue");
            
            case PatcherExitCode.PatchFailed:
                return Result.FromError("A patch failed to apply");
            
            default:
                return Result.FromError("An unknown error occurred in the patcher");
        }
    }
    
    public static ReadProcessResult RunAndReadProcessOutputs(string fileName, string args, int timeout = 5000)
    {
        using var proc = new Process();
        
        proc.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        
        using AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
        using AutoResetEvent errorWaitHandle = new AutoResetEvent(false);
        
        proc.OutputDataReceived += (s, e) =>
        {
            if (e.Data == null)
            {
                outputWaitHandle.Set();
            }
            else
            {
                outputBuilder.AppendLine(e.Data);
            }
        };
        
        proc.ErrorDataReceived += (s, e) =>
        {
            if (e.Data == null)
            {
                errorWaitHandle.Set();
            }
            else
            {
                errorBuilder.AppendLine(e.Data);
            }
        };
        
        try
        {
            proc.Start();
        }
        catch (Exception ex)
        {
            return ReadProcessResult.FromError(ex.Message);
        }
        
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();
        
        if (!proc.WaitForExit(timeout) || !outputWaitHandle.WaitOne(timeout) || !errorWaitHandle.WaitOne(timeout))
        {
            return ReadProcessResult.FromError("Process timed out");
        }
        
        return ReadProcessResult.FromSuccess(outputBuilder.ToString(), errorBuilder.ToString());
    }
}