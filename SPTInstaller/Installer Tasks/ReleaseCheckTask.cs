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
            var akiReleaseInfoFile =
                await DownloadCacheHelper.DownloadFileAsync("release.json", DownloadCacheHelper.ReleaseMirrorUrl,
                    progress);
            if (akiReleaseInfoFile == null)
            {
                return Result.FromError("Failed to download release metadata");
            }
            
            var akiReleaseInfo =
                JsonConvert.DeserializeObject<ReleaseInfo>(File.ReadAllText(akiReleaseInfoFile.FullName));
            
            SetStatus("Checking for Patches", "", null, ProgressStyle.Indeterminate);
            
            var akiPatchMirrorsFile =
                await DownloadCacheHelper.DownloadFileAsync("mirrors.json", DownloadCacheHelper.PatchMirrorUrl,
                    progress);
            
            if (akiPatchMirrorsFile == null)
            {
                return Result.FromError("Failed to download patch mirror data");
            }
            
            var patchMirrorInfo =
                JsonConvert.DeserializeObject<PatchInfo>(File.ReadAllText(akiPatchMirrorsFile.FullName));
            
            if (akiReleaseInfo == null || patchMirrorInfo == null)
            {
                return Result.FromError("An error occurred while deserializing aki or patch data");
            }
            
            _data.ReleaseInfo = akiReleaseInfo;
            _data.PatchInfo = patchMirrorInfo;
            int intAkiVersion = int.Parse(akiReleaseInfo.ClientVersion);
            int intGameVersion = int.Parse(_data.OriginalGameVersion);
            
            // note: it's possible the game version could be lower than the aki version and still need a patch if the major version numbers change
            //     : it's probably a low chance though
            bool patchNeedCheck = intGameVersion > intAkiVersion;
            
            if (intGameVersion < intAkiVersion)
            {
                return Result.FromError("Your client is outdated. Please update EFT");
            }
            
            if (intGameVersion == intAkiVersion)
            {
                patchNeedCheck = false;
            }
            
            if ((intGameVersion != patchMirrorInfo.SourceClientVersion ||
                 intAkiVersion != patchMirrorInfo.TargetClientVersion) && patchNeedCheck)
            {
                return Result.FromError(
                    "No patcher available for your version.\nA patcher is usually created within 24 hours of an EFT update.");
            }
            
            _data.PatchNeeded = patchNeedCheck;
            
            string status =
                $"Current Release: {akiReleaseInfo.ClientVersion} - {(_data.PatchNeeded ? "Patch Available" : "No Patch Needed")}";
            
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