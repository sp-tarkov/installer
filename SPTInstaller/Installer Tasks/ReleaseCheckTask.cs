﻿using SPTInstaller.Interfaces;
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
            var SPTReleaseInfoFile =
                await DownloadCacheHelper.GetOrDownloadFileAsync("release.json", DownloadCacheHelper.ReleaseMirrorUrl,
                    progress, DownloadCacheHelper.SuggestedTtl);
            
            if (SPTReleaseInfoFile == null)
            {
                return Result.FromError("Failed to download release metadata, try clicking the 'Whats this' button below followed by the 'Clear Metadata cache' button");
            }
            
            var SPTReleaseInfo =
                JsonConvert.DeserializeObject<ReleaseInfo>(File.ReadAllText(SPTReleaseInfoFile.FullName));
            
            SetStatus("Checking for Patches", "", null, ProgressStyle.Indeterminate);
            
            var SPTPatchMirrorsFile =
                await DownloadCacheHelper.GetOrDownloadFileAsync("mirrors.json", DownloadCacheHelper.PatchMirrorUrl,
                    progress, DownloadCacheHelper.SuggestedTtl);
            
            if (SPTPatchMirrorsFile == null)
            {
                return Result.FromError("Failed to download patch mirror data, try clicking the 'Whats this' button below followed by the 'Clear Metadata cache' button");
            }
            
            var patchMirrorInfo =
                JsonConvert.DeserializeObject<PatchInfo>(File.ReadAllText(SPTPatchMirrorsFile.FullName));
            
            if (SPTReleaseInfo == null || patchMirrorInfo == null)
            {
                return Result.FromError("An error occurred while deserializing SPT or patch data, try clicking the 'Whats this' button below followed by the 'Clear Metadata cache' button");
            }
            
            _data.ReleaseInfo = SPTReleaseInfo;
            _data.PatchInfo = patchMirrorInfo;
            int intSPTVersion = int.Parse(SPTReleaseInfo.ClientVersion);
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
                    "Could not find a downgrade patcher for the version of EFT you have installed." +
                    "\nThis can happen due to one of the following reasons:" +
                    "\n* Live EFT just updated. The SPT team will create a new patcher within 24 hours, hold tight!" +
                    "\n* Live EFT just updated. You have not installed it on your computer using your Battlestate Games launcher");
            }

            if (liveClientIsOutdated)
            {
                return Result.FromError("Your live EFT is out of date. Please update it using your Battlestate Games Launcher then run the SPT Installer again");
            }
            
            _data.PatchNeeded = patchNeedCheck;
            
            string status =
                $"Current Release: {SPTReleaseInfo.ClientVersion} - {(_data.PatchNeeded ? "Patch Available" : "No Patch Needed")}";
            
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