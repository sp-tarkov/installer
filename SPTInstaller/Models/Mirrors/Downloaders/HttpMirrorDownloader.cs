using SPTInstaller.Helpers;
using System.Threading.Tasks;

namespace SPTInstaller.Models.Mirrors.Downloaders;
public class HttpMirrorDownloader : MirrorDownloaderBase
{
    public HttpMirrorDownloader(DownloadMirror mirror) : base(mirror)
    {
    }

    public override async Task<FileInfo?> Download(IProgress<double> progress)
    {
        return await DownloadCacheHelper.DownloadFileAsync("patcher.zip", MirrorInfo.Link, progress);
    }
}
