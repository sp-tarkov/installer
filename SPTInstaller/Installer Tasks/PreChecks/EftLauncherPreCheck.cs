using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;
using SPTInstaller.Models;

namespace SPTInstaller.Installer_Tasks.PreChecks;

public class EftLauncherPreCheck : PreCheckBase
{
    public EftLauncherPreCheck() : base("EFT Launcher Closed", true)
    {
    }

    public async override Task<PreCheckResult> CheckOperation()
    {
        var eftLauncherProcs = Process.GetProcessesByName("BsgLauncher");
        
        return eftLauncherProcs.Length == 0
            ? PreCheckResult.FromSuccess("Eft launcher is closed")
            : PreCheckResult.FromError("Eft launcher is open. Please close it to install SPT", 
                "Kill EFT Launcher Processes", 
                () =>
            {
                var bsgLauncherProcs = Process.GetProcessesByName("BsgLauncher");

                foreach (var proc in bsgLauncherProcs)
                {
                    try
                    {
                        proc.Kill();
                        proc.WaitForExit();
                        Log.Information($"Killed Proc: {proc.ProcessName}#{proc.Id}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Failed to kill proc: {proc.ProcessName}#{proc.Id}");
                    }
                }
                
                RequestReevaluation();
            });
    }
}