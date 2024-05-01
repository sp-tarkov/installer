using System.Collections.Generic;

namespace SPTInstaller.Models;

public class InstallerInfo
{
    public string LatestVersion { get; set; }
    public List<string> Changes { get; set; }
}