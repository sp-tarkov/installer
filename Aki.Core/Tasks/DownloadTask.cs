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
            var mirrorListInfo = new FileInfo(Path.Join(_data.TargetInstallPath, "mirrors.json"));

            SetStatus("Downloading Mirror List", false);

            var progress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            var downloadResult = await DownloadHelper.DownloadFile(mirrorListInfo, _data.PatcherMirrorsLink, progress);

            if (!downloadResult.Succeeded)
            {
                return downloadResult;
            }

            var blah = JsonConvert.DeserializeObject<List<DownloadMirror>>(File.ReadAllText(mirrorListInfo.FullName));

            if (blah is List<DownloadMirror> mirrors)
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
                        using var patcherFileStream = _data.PatcherZipInfo.Open(FileMode.Create);
                        {
                            await megaDownloadStream.CopyToAsync(patcherFileStream);
                        }

                        patcherFileStream.Close();

                        if(!DownloadHelper.FileHashCheck(_data.PatcherZipInfo, mirror.Hash))
                        {
                            return GenericResult.FromError("Hash mismatch");
                        }

                        return GenericResult.FromSuccess();
                    }
                    catch (Exception)
                    {
                        //most likely a 509 (Bandwidth limit exceeded) due to mega's user quotas.
                        continue;
                    }
                }

                var result = await DownloadHelper.DownloadFile(_data.PatcherZipInfo, mirror.Link, progress, mirror.Hash);

                if (result.Succeeded)
                {
                    return GenericResult.FromSuccess();
                }
            }

            return GenericResult.FromError("Failed to download Patcher");
        }

        public override async Task<GenericResult> RunAsync()
        {
            _data.PatcherZipInfo = new FileInfo(Path.Join(_data.TargetInstallPath, "patcher.zip"));
            _data.AkiZipInfo = new FileInfo(Path.Join(_data.TargetInstallPath, "sptaki.zip"));

            if (_data.PatchNeeded)
            {
                if (_data.PatcherZipInfo.Exists) _data.PatcherZipInfo.Delete();

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

            if (_data.AkiZipInfo.Exists) _data.AkiZipInfo.Delete();

            SetStatus("Downloading SPT-AKI", false);

            Progress = 0;

            var akiProgress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            var releaseDownloadResult = await DownloadHelper.DownloadFile(_data.AkiZipInfo, _data.AkiReleaseDownloadLink, akiProgress);

            if (!releaseDownloadResult.Succeeded)
            {
                return releaseDownloadResult;
            }

            return GenericResult.FromSuccess();
        }
    }
}