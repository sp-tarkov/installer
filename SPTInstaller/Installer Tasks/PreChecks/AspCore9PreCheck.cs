using Serilog;
using SPTInstaller.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SPTInstaller.Helpers;

namespace SPTInstaller.Installer_Tasks.PreChecks;

public class AspCore9PreCheck : PreCheckBase
{
    public AspCore9PreCheck() : base("Asp.Net Core 9 Runtime", true)
    {
    }
    
    public override async Task<PreCheckResult> CheckOperation()
    {
        var minRequiredVersion = new Version("9.0.0");
        string[] output;
        
        var failedButtonText = "Download Asp.Net Core 9 Desktop Runtime";
        
        var failedButtonAction = () =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ArgumentList =
                {
                    "/C", "start",
                    "https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.9-windows-x64-installer"
                }
            });
        };
        
        try
        {
            var programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            var result =
                ProcessHelper.RunAndReadProcessOutputs($@"{programFiles}\dotnet\dotnet.exe", "--list-runtimes");
            
            if (!result.Succeeded)
            {
                return PreCheckResult.FromError(result.Message + "\n\nYou most likely don't have asp.net 9 installed",
                    failedButtonText, failedButtonAction);
            }
            
            output = result.StdOut.Split("\r\n");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"PreCheck::{Name}::Exception");
            return PreCheckResult.FromException(ex);
        }
        
        var highestFoundVersion = new Version("0.0.0");
        
        foreach (var lineVersion in output)
        {
            var regex = Regex.Match(lineVersion, @"Microsoft.AspNetCore.App (\d\.\d\.\d)");
            
            if (!regex.Success || regex.Groups.Count < 1)
                continue;
            
            var stringVersion = regex.Groups[1].Value;
            
            var foundVersion = new Version(stringVersion);
            
            if (foundVersion >= minRequiredVersion)
            {
                return PreCheckResult.FromSuccess(
                    $"Asp.Net Core {minRequiredVersion} Desktop Runtime or higher is installed.\n\nInstalled Version: {foundVersion}");
            }
            
            highestFoundVersion = foundVersion > highestFoundVersion ? foundVersion : highestFoundVersion;
        }
        
        return PreCheckResult.FromError(
            $"Asp.Net Core Desktop Runtime version {minRequiredVersion} or higher is required.\n\nHighest Version Found: {(highestFoundVersion > new Version("0.0.0") ? highestFoundVersion : "Not Found")}\n\nThis is required to play SPT, but you can install it later if and shouldn't affect the SPT install process.",
            failedButtonText, failedButtonAction);
    }
}