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
            var SPTReleaseInfoFile =
                await DownloadCacheHelper.GetOrDownloadFileAsync("release.json", DownloadCacheHelper.ReleaseMirrorUrl,
                    progress, DownloadCacheHelper.SuggestedTtl);
            
            if (SPTReleaseInfoFile == null)
            {
                return Result.FromError("Failed to download release metadata");
            }
            
            var SPTReleaseInfo =
                JsonConvert.DeserializeObject<ReleaseInfo>(File.ReadAllText(SPTReleaseInfoFile.FullName));
            
            SetStatus("Checking for Patches", "", null, ProgressStyle.Indeterminate);
            
            var SPTPatchMirrorsFile =
                await DownloadCacheHelper.GetOrDownloadFileAsync("mirrors.json", DownloadCacheHelper.PatchMirrorUrl,
                    progress, DownloadCacheHelper.SuggestedTtl);
            
            if (SPTPatchMirrorsFile == null)
            {
                return Result.FromError("Failed to download patch mirror data");
            }
            
            var patchMirrorInfo =
                JsonConvert.DeserializeObject<PatchInfo>(File.ReadAllText(SPTPatchMirrorsFile.FullName));
            
            if (SPTReleaseInfo == null || patchMirrorInfo == null)
            {
                return Result.FromError("An error occurred while deserializing SPT or patch data");
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
                return Result.FromError("Your client is outdated. Please update EFT");
            }
            
            if (intGameVersion == intSPTVersion)
            {
                patchNeedCheck = false;
            }
            
            if ((intGameVersion != patchMirrorInfo.SourceClientVersion ||
                 intSPTVersion != patchMirrorInfo.TargetClientVersion) && patchNeedCheck)
            {
                return Result.FromError(
                    "No patcher available for your version.\nA patcher is usually created within 24 hours of an EFT update.");
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