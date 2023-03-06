using CG.Web.MegaApiClient;
using Newtonsoft.Json;
using SPT_AKI_Installer.Aki.Core.Model;
using SPT_AKI_Installer.Aki.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
            SetStatus("Downloading Mirror List", false);

            var progress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            var file = await DownloadCacheHelper.GetOrDownloadFileAsync("mirrors.json", _data.PatcherMirrorsLink, progress);

            if (file == null)
            {
                return GenericResult.FromError("Failed to download mirror list");
            }

            var mirrorsList = JsonConvert.DeserializeObject<List<DownloadMirror>>(File.ReadAllText(file.FullName));

            if (mirrorsList is List<DownloadMirror> mirrors)
            {
                _data.PatcherReleaseMirrors = mirrors;

                return GenericResult.FromSuccess();
            }

            return GenericResult.FromError("Failed to deserialize mirrors list");
        }

        private async Task<GenericResult> DownloadPatcherFromMirrors(IProgress<double> progress)
        {
            foreach (var mirror in _data.PatcherReleaseMirrors)
            {
                SetStatus($"Downloading Patcher: {mirror.Link}", false);

                // mega is a little weird since they use encryption, but thankfully there is a great library for their api :)
                if (mirror.Link.StartsWith("https://mega"))
                {
                    var megaClient = new MegaApiClient();
                    await megaClient.LoginAnonymousAsync();

                    // if mega fails to connect, try the next mirror
                    if (!megaClient.IsLoggedIn) continue;

                    try
                    {
                        using var megaDownloadStream = await megaClient.DownloadAsync(new Uri(mirror.Link), progress);

                        _data.PatcherZipInfo = await DownloadCacheHelper.GetOrDownloadFileAsync("patcher.zip", megaDownloadStream, mirror.Hash);

                        if(_data.PatcherZipInfo == null)
                        {
                            continue;
                        }

                        return GenericResult.FromSuccess();
                    }
                    catch
                    {
                        //most likely a 509 (Bandwidth limit exceeded) due to mega's user quotas.
                        continue;
                    }
                }

                _data.PatcherZipInfo = await DownloadCacheHelper.GetOrDownloadFileAsync("patcher.zip", mirror.Link, progress, mirror.Hash);

                if (_data.PatcherZipInfo != null)
                {
                    return GenericResult.FromSuccess();
                }
            }

            return GenericResult.FromError("Failed to download Patcher");
        }

        public override async Task<GenericResult> RunAsync()
        {
            if (_data.PatchNeeded)
            {
                var buildResult = await BuildMirrorList();

                if (!buildResult.Succeeded)
                {
                    return buildResult;
                }

                Progress = 0;

                var progress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });
                var patcherDownloadRresult = await DownloadPatcherFromMirrors(progress);

                if (!patcherDownloadRresult.Succeeded)
                {
                    return patcherDownloadRresult;
                }
            }

            SetStatus("Downloading SPT-AKI", false);

            Progress = 0;

            var akiProgress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            _data.AkiZipInfo = await DownloadCacheHelper.GetOrDownloadFileAsync("sptaki.zip", _data.AkiReleaseDownloadLink, akiProgress, _data.AkiReleaseHash);

            if (_data.AkiZipInfo == null)
            {
                return GenericResult.FromError("Failed to download spt-aki");
            }

            return GenericResult.FromSuccess();
        }
    }
}