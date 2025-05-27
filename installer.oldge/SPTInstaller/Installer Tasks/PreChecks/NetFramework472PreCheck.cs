using Microsoft.Win32;
using Serilog;
using SPTInstaller.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SPTInstaller.Installer_Tasks.PreChecks;

public class NetFramework472PreCheck : PreCheckBase
{
    public NetFramework472PreCheck() : base(".Net Framework 4.7.2", true)
    {
    }
    
    public override async Task<PreCheckResult> CheckOperation()
    {
        try
        {
            var minRequiredVersion = new Version("4.7.2");
            
            var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full");
            
            var failedButtonText = "Download .Net Framework 4.7.2";
            
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
                        "https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net472-developer-pack-offline-installer"
                    }
                });
            };
            
            if (key == null)
            {
                return PreCheckResult.FromError(
                    "Could not find .Net Framework on system.\n\nThis is required to play SPT, but you can install it later and shouldn't affect the SPT install process.",
                    failedButtonText, failedButtonAction);
            }
            
            var value = key.GetValue("Version");
            
            if (value == null || value is not string versionString)
            {
                return PreCheckResult.FromError(
                    "Something went wrong. This precheck failed for an unknown reason.  :(");
            }
            
            var installedVersion = new Version(versionString);
            
            if (installedVersion < minRequiredVersion)
            {
                return PreCheckResult.FromError(
                    $".Net Framework {versionString} is installed, but {minRequiredVersion} or higher is required.\n\nYou can install it later and shouldn't affect the SPT install process.",
                    failedButtonText, failedButtonAction);
            }
            
            return PreCheckResult.FromSuccess(
                $".Net Framework {minRequiredVersion} or higher is installed.\n\nInstalled Version: {installedVersion}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"PreCheck::{Name}::Exception");
            return PreCheckResult.FromException(ex);
        }
    }
}