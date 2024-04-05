using Gitea.Api;
using Gitea.Client;
using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Threading.Tasks;
using SPTInstaller.Helpers;

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

            var akiRepoReleases = await repo.RepoListReleasesAsync("SPT-AKI", "Stable-releases");

            SetStatus("Checking for Patches", "", null, ProgressStyle.Indeterminate);

            var patchRepoReleases = await repo.RepoListReleasesAsync("SPT-AKI", "Downgrade-Patches");

            var latestAkiRelease = akiRepoReleases.FindAll(x => !x.Prerelease)[0];
            var latestAkiVersion = latestAkiRelease.Name.Replace('(', ' ').Replace(')', ' ').Split(' ')[3];
            var comparePatchToAki = patchRepoReleases?.Find(x => x.Name.Contains(_data.OriginalGameVersion) && x.Name.Contains(latestAkiVersion));

            _data.PatcherMirrorsLink = comparePatchToAki?.Assets[0].BrowserDownloadUrl;
            _data.AkiReleaseDownloadLink = latestAkiRelease.Assets[0].BrowserDownloadUrl;
            _data.AkiReleaseHash = FileHashHelper.GetGiteaReleaseHash(latestAkiRelease);

            int IntAkiVersion = int.Parse(latestAkiVersion);
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
                return Result.FromError("No patcher available for your version. A patcher is usually created within 24 hours of an EFT update.\nYou can join our discord and watch the dev-webhooks channel for '[SPT-AKI/Downgrade-Patches] Release created' to know when a patcher is available");
            }

            _data.PatchNeeded = patchNeedCheck;

            string status = $"Current Release: {latestAkiVersion} - {(_data.PatchNeeded ? "Patch Available" : "No Patch Needed")}";

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