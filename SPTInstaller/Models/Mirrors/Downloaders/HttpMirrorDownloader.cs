using SPTInstaller.Helpers;
using System.Threading.Tasks;

namespace SPTInstaller.Models.Mirrors.Downloaders;
public class HttpMirrorDownloader : MirrorDownloaderBase
{
    public HttpMirrorDownloader(PatchInfoMirror mirror) : base(mirror)
    {
    }

    public override async Task<FileInfo?> Download(IProgress<double> progress)
    {
        var file = await DownloadCacheHelper.DownloadFileAsync("patcher", MirrorInfo.Link, progress);

        if (file == null)
            return null;

        return FileHashHelper.CheckHash(file, MirrorInfo.Hash) ? file : null;
    }
}
