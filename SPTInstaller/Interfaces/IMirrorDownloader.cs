using SPTInstaller.Models.Mirrors;
using System.Threading.Tasks;

namespace SPTInstaller.Interfaces;
public interface IMirrorDownloader
{
    public DownloadMirror MirrorInfo { get; }
    public Task<FileInfo?> Download(IProgress<double> progress);
}
