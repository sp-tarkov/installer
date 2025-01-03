using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Threading.Tasks;
using SPTInstaller.Helpers;
using Newtonsoft.Json;
using SPTInstaller.Models.Mirrors;
using SPTInstaller.Models.ReleaseInfo;

namespace SPTInstaller.Installer_Tasks;

public class ReleaseCheckTask : InstallerTaskBase
{
    private InternalData _data;
    
    public ReleaseCheckTask(InternalData data) : base("Release Checks")
    {
        _data = data;
    }
    
    public override async Task<IResult> TaskOperation()
    {
        try
        {
            SetStatus("Checking SPT Releases", "", null, ProgressStyle.Indeterminate);
            
            var progress = new Progress<double>((d) => { SetStatus(null, null, (int)Math.Floor(d)); });
            
            ReleaseInfo? sptReleaseInfo = null;
            PatchInfo? patchMirrorInfo = null;
            
            int retries = 1;

            while (retries >= 0)
            {
                retries--;
                
                try
                {
                    var sptReleaseInfoFile =
                        await DownloadCacheHelper.GetOrDownloadFileAsync("release.json", DownloadCacheHelper.ReleaseMirrorUrl,
                            progress, DownloadCacheHelper.SuggestedTtl);
            
                    if (sptReleaseInfoFile == null)
                    {
                        return Result.FromError("Failed to download release metadata, try clicking the 'Whats this' button below followed by the 'Clear Metadata cache' button");
                    }
            
                    SetStatus("Checking for Patches", "", null, ProgressStyle.Indeterminate);
            
                    var sptPatchMirrorsFile =
                        await DownloadCacheHelper.GetOrDownloadFileAsync("mirrors.json", DownloadCacheHelper.PatchMirrorUrl,
                            progress, DownloadCacheHelper.SuggestedTtl);
            
                    if (sptPatchMirrorsFile == null)
                    {
                        return Result.FromError("Failed to download patch mirror data, try clicking the 'Whats this' button below followed by the 'Clear Metadata cache' button");
                    }
                    
                    sptReleaseInfo =
                        JsonConvert.DeserializeObject<ReleaseInfo>(File.ReadAllText(sptReleaseInfoFile.FullName));

                    patchMirrorInfo =
                        JsonConvert.DeserializeObject<PatchInfo>(File.ReadAllText(sptPatchMirrorsFile.FullName));

                    break;
                }
                catch (Exception ex)
                {
                    if (retries >= 0)
                    {
                        SetStatus("Clearing cache and retrying ...", "", null, ProgressStyle.Indeterminate);
                        await Task.Delay(1000);
                        DownloadCacheHelper.ClearMetadataCache();
                        continue;
                    }
                    
                    return Result.FromError(
                        $"An error occurred while deserializing SPT or patch data.\n\nMost likely we are uploading a new patch.\nPlease wait and try again in an hour\n\nERROR: {ex.Message}");
                }
            }

            if (sptReleaseInfo == null || patchMirrorInfo == null)
            {
                return Result.FromError(
                    "Release or mirror info was null. If you are seeing this report it. This should never be hit");
            }

            _data.ReleaseInfo = sptReleaseInfo;
            _data.PatchInfo = patchMirrorInfo;
            int intSPTVersion = int.Parse(sptReleaseInfo.ClientVersion);
            int intGameVersion = int.Parse(_data.OriginalGameVersion);
            
            // note: it's possible the game version could be lower than the SPT version and still need a patch if the major version numbers change
            //     : it's probably a low chance though
            bool patchNeedCheck = intGameVersion > intSPTVersion;
            
            if (intGameVersion < intSPTVersion)
            {
                return Result.FromError("Your live EFT is out of date. Please update it using the Battlestate Games Launcher and try runing the SPT Installer again");
            }
            
            if (intGameVersion == intSPTVersion)
            {
                patchNeedCheck = false;
            }
            
            /*
              An example of the logic going on here because holy shit I can't keep track of why we do it this way -waffle.lazy
              ----    Example data    ----
              gameVersion         : 32738
              sptVersion          : 30626
              SourceClientVersion : 32678
              TargetClientVersion : 30626
              patchNeeded         : true
              ----------------------------
              
              * spt client is 'outdated' if the game and target versions don't match
              * or
              * the game version is behind the mirror's source client version
              sptClientIsOutdated  = (30626 != 30626 || 32738 > 32678) && true
              
              * otherwise, if the game version doesn't match the mirror's source version, we assume live is outdated 
              liveClientIsOutdated = 32738 != 32678 && true
             */

            bool sptClientIsOutdated = (intSPTVersion != patchMirrorInfo.TargetClientVersion || intGameVersion > patchMirrorInfo.SourceClientVersion) && patchNeedCheck;
            bool liveClientIsOutdated = intGameVersion != patchMirrorInfo.SourceClientVersion && patchNeedCheck;
            
            if (sptClientIsOutdated)
            {
                return Result.FromError(
                    "Live EFT has recently updated. The SPT team needs to make a new patcher." +
                    "\n* It's usually made within 24 hours." +
                    "\n* The patcher is only for turning your EFT files into an older version for SPT to use." +
                    "\n* This does not mean SPT is being updated to a newer version.");
            }

            if (liveClientIsOutdated)
            {
                return Result.FromError("Your live EFT is out of date. Please update it using your Battlestate Games Launcher then run the SPT Installer again");
            }
            
            _data.PatchNeeded = patchNeedCheck;
            
            string status =
                $"Current Release: {sptReleaseInfo.ClientVersion} - {(_data.PatchNeeded ? "Patch Available" : "No Patch Needed")}";
            
            SetStatus(null, status);
            
            return Result.FromSuccess(status);
        }
        catch (Exception ex)
        {
            //request failed
            return Result.FromError($"Request Failed:\n{ex.Message}");
        }
    }
}