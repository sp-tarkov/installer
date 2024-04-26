using Gitea.Api;
using Gitea.Client;
using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Threading.Tasks;
using SPTInstaller.Helpers;
using Newtonsoft.Json;
using SPTInstaller.Models.Releases;

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
            var repo = new RepositoryApi(Configuration.Default);

            SetStatus("Checking SPT Releases", "", null, ProgressStyle.Indeterminate);

            var progress = new Progress<double>((d) => { SetStatus(null, null, (int)Math.Floor(d)); });
            var akiReleaseInfoFile = await DownloadCacheHelper.DownloadFileAsync("release.json", "https://spt-releases.modd.in/release.json", progress);
            if (akiReleaseInfoFile == null)
            {
                return Result.FromError("Failed to download release metadata");
            }

            var akiReleaseInfo = JsonConvert.DeserializeObject<ReleaseInfo>(File.ReadAllText(akiReleaseInfoFile.FullName));

            SetStatus("Checking for Patches", "", null, ProgressStyle.Indeterminate);

            var patchRepoReleases = await repo.RepoListReleasesAsync("SPT-AKI", "Downgrade-Patches");

            var comparePatchToAki = patchRepoReleases?.Find(x => x.Name.Contains(_data.OriginalGameVersion) && x.Name.Contains(akiReleaseInfo.ClientVersion));

            _data.PatcherMirrorsLink = comparePatchToAki?.Assets[0].BrowserDownloadUrl;
            _data.ReleaseInfo = akiReleaseInfo;

            int IntAkiVersion = int.Parse(akiReleaseInfo.ClientVersion);
            int IntGameVersion = int.Parse(_data.OriginalGameVersion);
            bool patchNeedCheck = false;

            if (IntGameVersion > IntAkiVersion)
            {
                patchNeedCheck = true;
            }

            if (IntGameVersion < IntAkiVersion)
            {
                return Result.FromError("Your client is outdated. Please update EFT");

            }

            if (IntGameVersion == IntAkiVersion)
            {
                patchNeedCheck = false;
            }

            if (comparePatchToAki == null && patchNeedCheck)
            {
                return Result.FromError("No patcher available for your version.\nA patcher is usually created within 24 hours of an EFT update.");
            }

            _data.PatchNeeded = patchNeedCheck;

            string status = $"Current Release: {akiReleaseInfo.ClientVersion} - {(_data.PatchNeeded ? "Patch Available" : "No Patch Needed")}";

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