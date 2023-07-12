using Microsoft.Win32;
using SPTInstaller.Models;
using System.Threading.Tasks;

namespace SPTInstaller.Installer_Tasks.PreChecks;

public class NetFramework472PreCheck : PreCheckBase
{
    public NetFramework472PreCheck() : base(".Net Framework 4.7.2", false)
    {
    }

    public override async Task<bool> CheckOperation()
    {
        try
        {
            var minRequiredVersion = new Version("4.7.2");

            var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full");

            if (key == null)
            {
                return false;
            }

            var value = key.GetValue("Version");

            if (value != null && value is string versionString)
            {
                var installedVersion = new Version(versionString);

                return installedVersion > minRequiredVersion;
            }

            return false;
        }
        catch (Exception ex)
        {
            // TODO: log exceptions

            return false;
        }
    }
}