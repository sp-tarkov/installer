using Serilog;
using SPTInstaller.Helpers;
using SPTInstaller.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SPTInstaller.Installer_Tasks.PreChecks;

public class Net9PreCheck : PreCheckBase
{
    public Net9PreCheck() : base(".Net 9 Desktop Runtime", true)
    {
    }
    
    public override async Task<PreCheckResult> CheckOperation()
    {
        var minRequiredVersion = new Version("9.0.9");
        string[] output;
        
        const string failedButtonText = "Download .Net 9 Desktop Runtime";
        
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
                    "https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-9.0.9-windows-x64-installer"
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
                return PreCheckResult.FromError(result.Message + "\n\nYou most likely don't have .net 9 installed",
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
            var regex = Regex.Match(lineVersion, @"Microsoft.WindowsDesktop.App (\d\.\d\.\d)");
            
            if (!regex.Success || regex.Groups.Count < 1)
                continue;
            
            var stringVersion = regex.Groups[1].Value;
            
            var foundVersion = new Version(stringVersion);
            
            if (foundVersion >= minRequiredVersion)
            {
                return PreCheckResult.FromSuccess(
                    $".Net {minRequiredVersion} Desktop Runtime or higher is installed.\n\nInstalled Version: {foundVersion}");
            }
            
            highestFoundVersion = foundVersion > highestFoundVersion ? foundVersion : highestFoundVersion;
        }
        
        return PreCheckResult.FromError(
            $".Net Desktop Runtime version {minRequiredVersion} or higher is required.\n\nHighest Version Found: {(highestFoundVersion > new Version("0.0.0") ? highestFoundVersion : "Not Found")}\n\nThis is required to play SPT",
            failedButtonText, failedButtonAction);
    }
}