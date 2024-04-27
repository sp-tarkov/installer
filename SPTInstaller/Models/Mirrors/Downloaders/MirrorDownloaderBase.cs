using SPTInstaller.Interfaces;
using System.Threading.Tasks;

namespace SPTInstaller.Models.Mirrors.Downloaders;
public abstract class MirrorDownloaderBase : IMirrorDownloader
{
    public PatchInfoMirror MirrorInfo { get; private set; }
    public abstract Task<FileInfo?> Download(IProgress<double> progress);
    public MirrorDownloaderBase(PatchInfoMirror mirrorInfo)
    {
        MirrorInfo = mirrorInfo;
    }
}
