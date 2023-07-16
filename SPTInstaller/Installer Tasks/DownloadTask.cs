using CG.Web.MegaApiClient;
using Newtonsoft.Json;
using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SPTInstaller.Helpers;

namespace SPTInstaller.Installer_Tasks;

public class DownloadTask : InstallerTaskBase
{
    private InternalData _data;

    public DownloadTask(InternalData data) : base("Download Files")
    {
        _data = data;
    }

    private async Task<IResult> BuildMirrorList()
    {
        var progress = new Progress<double>((d) => { SetStatus("Downloading Mirror List", "", (int)Math.Floor(d));});

        var file = await DownloadCacheHelper.GetOrDownloadFileAsync("mirrors.json", _data.PatcherMirrorsLink, progress);

        if (file == null)
        {
            return Result.FromError("Failed to download mirror list");
        }

        var mirrorsList = JsonConvert.DeserializeObject<List<DownloadMirror>>(File.ReadAllText(file.FullName));

        if (mirrorsList is List<DownloadMirror> mirrors)
        {
            _data.PatcherReleaseMirrors = mirrors;

            return Result.FromSuccess();
        }

        return Result.FromError("Failed to deserialize mirrors list");
    }

    private async Task<IResult> DownloadPatcherFromMirrors(IProgress<double> progress)
    {
        foreach (var mirror in _data.PatcherReleaseMirrors)
        {
            SetStatus($"Downloading Patcher", mirror.Link);

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

                    return Result.FromSuccess();
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
                return Result.FromSuccess();
            }
        }

        return Result.FromError("Failed to download Patcher");
    }

    public override async Task<IResult> TaskOperation()
    {
        var progress = new Progress<double>((d) => { SetStatus(null, null, (int)Math.Floor(d)); });

        if (_data.PatchNeeded)
        {
            var buildResult = await BuildMirrorList();

            if (!buildResult.Succeeded)
            {
                return buildResult;
            }

            SetStatus(null, null, 0);

            var patcherDownloadRresult = await DownloadPatcherFromMirrors(progress);

            if (!patcherDownloadRresult.Succeeded)
            {
                return patcherDownloadRresult;
            }
        }

        SetStatus("Downloading SPT-AKI", _data.AkiReleaseDownloadLink, 0);

        _data.AkiZipInfo = await DownloadCacheHelper.GetOrDownloadFileAsync("sptaki.zip", _data.AkiReleaseDownloadLink, progress, _data.AkiReleaseHash);

        if (_data.AkiZipInfo == null)
        {
            return Result.FromError("Failed to download spt-aki");
        }

        return Result.FromSuccess();
    }
}