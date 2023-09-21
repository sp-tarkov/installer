using SPTInstaller.Interfaces;
using System.Threading.Tasks;

namespace SPTInstaller.Models.Mirrors.Downloaders;
public abstract class MirrorDownloaderBase : IMirrorDownloader
{
    public DownloadMirror MirrorInfo { get; private set; }
    public abstract Task<FileInfo?> Download(IProgress<double> progress);
    public MirrorDownloaderBase(DownloadMirror mirrorInfo)
    {
        MirrorInfo = mirrorInfo;
    }
}
