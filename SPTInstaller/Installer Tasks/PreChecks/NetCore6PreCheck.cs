using SPTInstaller.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SPTInstaller.Installer_Tasks.PreChecks;

public class NetCore6PreCheck : PreCheckBase
{
    public NetCore6PreCheck() : base(".Net Core 6 Desktop Runtime", false)
    {
    }

    public override async Task<PreCheckResult> CheckOperation()
    {
        var minRequiredVersion = new Version("6.0.0");
        string[] output;

        var failedButtonText = "Download .Net Core 6 Desktop Runtime";

        var failedButtonAction = () =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ArgumentList = { "/C", "start", "https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.4-windows-x64-installer" }
            });
        };

        try
        {
            var proc = Process.Start(new ProcessStartInfo()
            {
                FileName = "dotnet",
                Arguments = "--list-runtimes",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            proc.WaitForExit();

            output = proc.StandardOutput.ReadToEnd().Split("\r\n");
        }
        catch (Exception ex)
        {
            // TODO: logging
            return PreCheckResult.FromException(ex);
        }

        var highestFoundVersion = new Version("0.0.0");

        foreach (var lineVersion in output)
        {
            if (lineVersion.StartsWith("Microsoft.WindowsDesktop.App") && lineVersion.Split(" ").Length > 1)
            {
                string stringVerion = lineVersion.Split(" ")[1];

                var foundVersion = new Version(stringVerion);

                // waffle: not fully sure if we should only check for 6.x.x versions or if higher major versions are ok
                if (foundVersion >= minRequiredVersion)
                {
                    return PreCheckResult.FromSuccess($".Net Core {minRequiredVersion} Desktop Runtime or higher is installed.\n\nInstalled Version: {foundVersion}");
                }

                highestFoundVersion = foundVersion > highestFoundVersion ? foundVersion : highestFoundVersion;
            }
        }

        return PreCheckResult.FromError($".Net Core Desktop Runtime version {minRequiredVersion} or higher is required.\n\nHighest Version Found: {(highestFoundVersion > new Version("0.0.0") ? highestFoundVersion : "Not Found")}\n\nThis is required to play SPT, but you can install it later if and shouldn't affect the SPT install process.", failedButtonText, failedButtonAction);
    }
}