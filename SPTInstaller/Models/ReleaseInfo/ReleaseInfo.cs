using System.Collections.Generic;

namespace SPTInstaller.Models.ReleaseInfo;

public class ReleaseInfo
{
    public string AkiVersion { get; set; }
    public string ClientVersion { get; set; }
    public List<ReleaseInfoMirror> Mirrors { get; set; }
}