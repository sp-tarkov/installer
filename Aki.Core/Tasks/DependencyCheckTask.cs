using Microsoft.Win32;
using SPT_AKI_Installer.Aki.Core.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Core.Tasks
{
    public class DependencyCheckTask : LiveTableTask
    {
        private bool CheckNetCore6Installed()
        {
            var minRequiredVersion = new Version("6.0.0");
            string[] output;

            try
            {
                var proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    RedirectStandardOutput = true
                });

                proc.WaitForExit();

                output = proc.StandardOutput.ReadToEnd().Split("\r\n");
            }
            catch
            {
                return false;
            }

            foreach(var lineVersion in output)
            {
                if (lineVersion.StartsWith("Microsoft.WindowsDesktop.App") && lineVersion.Split(" ").Length > 1)
                {
                    string stringVerion = lineVersion.Split(" ")[1];

                    var foundVersion = new Version(stringVerion);

                    // not fully sure if we should only check for 6.x.x versions or if higher major versions are ok -waffle
                    if(foundVersion >= minRequiredVersion)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckNet472Installed()
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

        public DependencyCheckTask() : base("Dependency Checks", true)
        {
        }

        GenericResult getResult(bool net472Check, bool netCoreCheck) =>
            (net472Check, netCoreCheck) switch
            {
                (true, true) => GenericResult.FromSuccess("Dependencies already installed"),
                (false, true) => GenericResult.FromWarning(".Net Framework 472 not found.\nCheck SPT release page for requirements\nhttps://hub.sp-tarkov.com/files/file/16-spt-aki/"),
                (true, false) => GenericResult.FromWarning(".Net Runtime Desktop 6 not found.\nCheck SPT release page for requirements\nhttps://hub.sp-tarkov.com/files/file/16-spt-aki/"),
                (false, false) => GenericResult.FromWarning("Required dependencies not found.\nCheck SPT release page for requirements\nhttps://hub.sp-tarkov.com/files/file/16-spt-aki/")
            };

        public override Task<GenericResult> RunAsync()
        {
            SetStatus("Checking for net framework");

            var net472Check = CheckNet472Installed();

            SetStatus("Checking for net core");

            var netCoreCheck = CheckNetCore6Installed();

            return Task.FromResult(getResult(net472Check, netCoreCheck));
        }
    }
}
