using SPTInstaller.Models.Mirrors;
using System.Threading.Tasks;

namespace SPTInstaller.Interfaces;

public interface IMirrorDownloader
{
    public PatchInfoMirror MirrorInfo { get; }
    public Task<FileInfo?> Download(IProgress<double> progress);
}