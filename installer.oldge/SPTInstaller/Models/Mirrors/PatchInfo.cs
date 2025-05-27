using System.Collections.Generic;

namespace SPTInstaller.Models.Mirrors;

public class PatchInfo
{
    public int SourceClientVersion { get; set; }
    public int TargetClientVersion { get; set; }
    public List<PatchInfoMirror> Mirrors { get; set; }
}