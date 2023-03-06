using Gitea.Api;
using Gitea.Client;
using Gitea.Model;
using SPT_AKI_Installer.Aki.Core.Model;
using SPT_AKI_Installer.Aki.Helper;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Core.Tasks
{
    public class ReleaseCheckTask : LiveTableTask
    {
        private InternalData _data;

        public ReleaseCheckTask(InternalData data) : base("Release Checks")
        {
            _data = data;
        }

        public override async Task<GenericResult> RunAsync()
        {
            Configuration.Default.BasePath = "https://dev.sp-tarkov.com/api/v1";

            var repo = new RepositoryApi(Configuration.Default);

            try
            {
                SetStatus("Checking SPT Releases", false);

                var akiRepoReleases = await repo.RepoListReleasesAsync("SPT-AKI", "Stable-releases");

                SetStatus("Checking for Patches", false);

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
                    return GenericResult.FromError("Your client is outdated. Please update EFT");

                }

                if (IntGameVersion == IntAkiVersion)
                {
                    patchNeedCheck = false;
                }

                if (comparePatchToAki == null && patchNeedCheck)
                {
                    return GenericResult.FromError("No patcher available for your version");
                }

                _data.PatchNeeded = patchNeedCheck;

                string status = $"Current Release: {latestAkiVersion}";

                if (_data.PatchNeeded)
                {
                    status += " - Patch Available";
                }

                return GenericResult.FromSuccess(status);
            }
            catch (Exception)
            {
                //request failed
                return GenericResult.FromError("Request Failed");
            }
        }
    }
}
