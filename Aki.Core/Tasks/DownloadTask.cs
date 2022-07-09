using Newtonsoft.Json;
using SPT_AKI_Installer.Aki.Core.Model;
using SPT_AKI_Installer.Aki.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;

namespace SPT_AKI_Installer.Aki.Core.Tasks
{
    public class DownloadTask : LiveTableTask
    {
        private InternalData _data;

        public DownloadTask(InternalData data) : base("Download Files", false)
        {
            _data = data;
        }

        private async Task<GenericResult> BuildMirrorList()
        {
            var mirrorListInfo = new FileInfo(Path.Join(_data.TargetInstallPath, "mirrors.json"));

            SetStatus("Downloading mirror list", false);

            var progress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            var downloadResult = await DownloadHelper.DownloadFile(mirrorListInfo, _data.PatcherMirrorsLink, progress);

            if (!downloadResult.Succeeded)
            {
                return downloadResult;
            }

            var blah = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(mirrorListInfo.FullName));

            if (blah is List<string> mirrors)
            {
                _data.PatcherReleaseMirrors = mirrors;

                return GenericResult.FromSuccess();
            }

            return GenericResult.FromError("Failed to deserialize mirrors list");
        }

        private async Task<GenericResult> DownloadPatcherFromMirrors(FileInfo patcherZip, IProgress<double> progress)
        {
            foreach (string mirror in _data.PatcherReleaseMirrors)
            {
                SetStatus($"Download Patcher: {mirror}", false);

                // mega is a little weird since they use encryption, but thankfully there is a great library for their api :)
                if (mirror.StartsWith("https://mega"))
                {
                    var megaClient = new MegaApiClient();
                    await megaClient.LoginAnonymousAsync();

                    // if mega fails to connect, try the next mirror
                    if (!megaClient.IsLoggedIn) continue;

                    try
                    {
                        using var megaDownloadStream = await megaClient.DownloadAsync(new Uri(mirror), progress);
                        using var patcherFileStream = patcherZip.Open(FileMode.Create);
                        {
                            await megaDownloadStream.CopyToAsync(patcherFileStream);
                        }

                        return GenericResult.FromSuccess();
                    }
                    catch(Exception)
                    {
                        //most likely a 509 (Bandwidth limit exceeded) due to mega's user quotas.
                        continue;
                    }
                }

                var result = await DownloadHelper.DownloadFile(patcherZip, mirror, progress);

                if(result.Succeeded)
                {
                    return GenericResult.FromSuccess();
                }
            }

            return GenericResult.FromError("Failed to download Patcher");
        }

        public override async Task<GenericResult> RunAsync()
        {
            var patcherZipInfo = new FileInfo(Path.Join(_data.TargetInstallPath, "patcher.zip"));
            var akiZipInfo = new FileInfo(Path.Join(_data.TargetInstallPath, "sptaki.zip"));

            if (_data.PatchNeeded)
            {
                if (patcherZipInfo.Exists) patcherZipInfo.Delete();

                var buildResult = await BuildMirrorList();

                if (!buildResult.Succeeded)
                {
                    return buildResult;
                }

                SetStatus("Downloading Patcher", false);
                
                Progress = 0;

                var progress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });
                var patcherDownloadRresult = await DownloadPatcherFromMirrors(patcherZipInfo, progress);

                if (!patcherDownloadRresult.Succeeded)
                {
                    return patcherDownloadRresult;
                }
            }

            if (akiZipInfo.Exists) akiZipInfo.Delete();

            SetStatus("Downloading SPT-AKI", false);

            Progress = 0;
                
            var akiProgress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            var releaseDownloadResult = await DownloadHelper.DownloadFile(akiZipInfo, _data.AkiReleaseDownloadLink, akiProgress);

            if (!releaseDownloadResult.Succeeded)
            {
                return releaseDownloadResult;
            }

            return GenericResult.FromSuccess();
        }
    }
}