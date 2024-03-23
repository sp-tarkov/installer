using Newtonsoft.Json;
using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SPTInstaller.Helpers;
using SPTInstaller.Models.Mirrors;
using SPTInstaller.Models.Mirrors.Downloaders;
using Serilog;

namespace SPTInstaller.Installer_Tasks;

public class DownloadTask : InstallerTaskBase
{
    private InternalData _data;
    private List<IMirrorDownloader> _mirrors = new List<IMirrorDownloader>();
    private string _expectedPatcherHash = "";

    public DownloadTask(InternalData data) : base("Download Files")
    {
        _data = data;
    }

    private async Task<IResult> BuildMirrorList()
    {
        var progress = new Progress<double>((d) => { SetStatus("Downloading Mirror List", "", (int)Math.Floor(d), ProgressStyle.Shown);});

        var file = await DownloadCacheHelper.DownloadFileAsync("mirrors.json", _data.PatcherMirrorsLink, progress);

        if (file == null)
        {
            return Result.FromError("Failed to download mirror list");
        }

        var mirrorsList = JsonConvert.DeserializeObject<List<DownloadMirror>>(File.ReadAllText(file.FullName));

        if (mirrorsList == null)
        {
            return Result.FromError("Failed to deserialize mirrors list");
        }

        foreach (var mirror in mirrorsList)
        {
            _expectedPatcherHash = mirror.Hash;

            switch (mirror.Link)
            {
                case string l when l.StartsWith("https://mega"):
                    _mirrors.Add(new MegaMirrorDownloader(mirror));
                    break;
                default:
                    _mirrors.Add(new HttpMirrorDownloader(mirror));
                    break;
            }
        }

        return Result.FromSuccess("Mirrors list ready");
    }

    private async Task<IResult> DownloadPatcherFromMirrors(IProgress<double> progress)
    {
        SetStatus("Downloading Patcher", "Verifying cached patcher ...", progressStyle: ProgressStyle.Indeterminate);

        if (DownloadCacheHelper.CheckCache("patcher", _expectedPatcherHash, out var cacheFile))
        {
            _data.PatcherZipInfo = cacheFile;
            Log.Information("Using cached file {fileName} - Hash: {hash}", _data.PatcherZipInfo.Name, _expectedPatcherHash);
            return Result.FromSuccess();
        }

        foreach (var mirror in _mirrors)
        {
            SetStatus("Downloading Patcher", mirror.MirrorInfo.Link, progressStyle: ProgressStyle.Indeterminate);

            _data.PatcherZipInfo = await mirror.Download(progress);

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

        _data.AkiZipInfo = await DownloadCacheHelper.GetOrDownloadFileAsync("sptaki", _data.AkiReleaseDownloadLink, progress, _data.AkiReleaseHash);

        if (_data.AkiZipInfo == null)
        {
            return Result.FromError("Failed to download spt-aki");
        }

        return Result.FromSuccess();
    }
}