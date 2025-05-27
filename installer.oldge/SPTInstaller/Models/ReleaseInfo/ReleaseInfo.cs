using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPTInstaller.Models.ReleaseInfo;

public class ReleaseInfo
{
    [JsonProperty("AkiVersion")]  // TODO: Change this and what gets uploaded to SPTVersion
    public string SPTVersion { get; set; }
    public string ClientVersion { get; set; }
    public List<ReleaseInfoMirror> Mirrors { get; set; }
}