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
        foreach (var mirror in _data.PatchInfo.Mirrors)
        {
            _expectedPatcherHash = mirror.Hash;
            
            switch (mirror.Link)
            {
                case { } l when l.StartsWith("https://mega"):
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
            Log.Information("Using cached file {fileName} - Hash: {hash}", _data.PatcherZipInfo.Name,
                _expectedPatcherHash);
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
    
    private async Task<IResult> DownloadSptAkiFromMirrors(IProgress<double> progress)
    {
        // Note that GetOrDownloadFileAsync handles the cached file hash check, so we don't need to check it first
        foreach (var mirror in _data.ReleaseInfo.Mirrors)
        {
            SetStatus("Downloading SPT-AKI", mirror.DownloadUrl, progressStyle: ProgressStyle.Indeterminate);
            
            _data.AkiZipInfo =
                await DownloadCacheHelper.GetOrDownloadFileAsync("sptaki", mirror.DownloadUrl, progress, mirror.Hash);
            
            if (_data.AkiZipInfo != null)
            {
                return Result.FromSuccess();
            }
        }
        
        return Result.FromError("Failed to download spt-aki");
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
        
        return await DownloadSptAkiFromMirrors(progress);
    }
}