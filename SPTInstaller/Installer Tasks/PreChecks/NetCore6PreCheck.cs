using SPTInstaller.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SPTInstaller.Installer_Tasks.PreChecks;

public class NetCore6PreCheck : PreCheckBase
{
    public NetCore6PreCheck() : base(".Net Core 6 Desktop Runtime", false)
    {
    }

    public override async Task<bool> CheckOperation()
    {
        var minRequiredVersion = new Version("6.0.0");
        string[] output;

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
            return false;
        }

        foreach (var lineVersion in output)
        {
            if (lineVersion.StartsWith("Microsoft.WindowsDesktop.App") && lineVersion.Split(" ").Length > 1)
            {
                string stringVerion = lineVersion.Split(" ")[1];

                var foundVersion = new Version(stringVerion);

                // not fully sure if we should only check for 6.x.x versions or if higher major versions are ok -waffle
                if (foundVersion >= minRequiredVersion)
                {
                    return true;
                }
            }
        }

        return false;
    }
}