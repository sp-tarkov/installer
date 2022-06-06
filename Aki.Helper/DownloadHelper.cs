using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Gitea.Api;
using Gitea.Client;
using Gitea.Model;
using System.Collections.Generic;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class DownloadHelper
    {
        public static bool patchNeedCheck;
        public static string akiLink;
        public static string patcherLink;

        public static async Task DownloadFileAsync(string targetFilePath, string targetLink, string newFileName)
        {
            using (var httpClient = new HttpClient())
            {
                await httpClient.DownloadFile(targetLink, Path.Join(targetFilePath, newFileName));
            }
        }

        public static async Task ReleaseCheck()
        {
            Configuration.Default.BasePath = "https://dev.sp-tarkov.com/api/v1";

            var repo = new RepositoryApi(Configuration.Default);

            try
            {
                var patchRepoReleases = await repo.RepoListReleasesAsync("SPT-AKI", "Downgrade-Patches");
                var akiRepoReleases = await repo.RepoListReleasesAsync("SPT-AKI", "Stable-releases");

                var latestAkiRelease = akiRepoReleases.FindAll(x => !x.Prerelease)[0];
                var latestAkiVersion = latestAkiRelease.Name.Replace('(', ' ').Replace(')', ' ').Split(' ')[3];
                var comparePatchToAki = patchRepoReleases?.Find(x => x.Name.Contains(PreCheckHelper.gameVersion) && x.Name.Contains(latestAkiVersion));

                patcherLink = comparePatchToAki?.Assets[0].BrowserDownloadUrl;
                akiLink = latestAkiRelease.Assets[0].BrowserDownloadUrl;

                int IntAkiVersion = int.Parse(latestAkiVersion);
                int IntGameVersion = int.Parse(PreCheckHelper.gameVersion);

                if (IntGameVersion > IntAkiVersion)
                {
                    patchNeedCheck = true;
                }

                if (IntGameVersion < IntAkiVersion)
                {
                    LogHelper.Info("Client is older than current Aki Version, Please update!");
                    LogHelper.Warning("Press enter to close the app!");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                if (IntGameVersion == IntAkiVersion)
                {
                    patchNeedCheck = false;
                }

                if(comparePatchToAki == null && patchNeedCheck)
                {
                    LogHelper.Warning("There is no current patcher for your client version and its needed");
                    LogHelper.Warning("Please try again later once a new patcher is uploaded.");
                    LogHelper.Warning("Press enter to close the app!");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
            catch (Exception)
            {
                //request failed
                LogHelper.Info("Request Failed");
            }
        }

        public static async Task DownloadFile(this HttpClient client, string address, string fileName)
        {
            using (var response = await client.GetAsync(address))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var file = File.OpenWrite(fileName))
            {
                stream.CopyTo(file);
            }
        }
    }
}
