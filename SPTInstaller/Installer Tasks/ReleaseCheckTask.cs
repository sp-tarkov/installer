using Gitea.Api;
using Gitea.Client;
using SPTInstaller.Aki.Helper;
using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System;
using System.Threading.Tasks;

namespace SPTInstaller.Installer_Tasks
{
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
                Configuration.Default.BasePath = "https://dev.sp-tarkov.com/api/v1";

                var repo = new RepositoryApi(Configuration.Default);

                SetStatus("Checking SPT Releases");

                var akiRepoReleases = await repo.RepoListReleasesAsync("SPT-AKI", "Stable-releases");

                SetStatus("Checking for Patches");

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
                    return Result.FromError("No patcher available for your version");
                }

                _data.PatchNeeded = patchNeedCheck;

                string status = $"Current Release: {latestAkiVersion}";

                if (_data.PatchNeeded)
                {
                    status += " - Patch Available";
                }

                return Result.FromSuccess(status);
            }
            catch (Exception ex)
            {
                //request failed
                return Result.FromError($"Request Failed:\n{ex.Message}");
            }
        }
    }
}
