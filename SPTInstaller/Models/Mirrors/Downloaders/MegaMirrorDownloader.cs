using CG.Web.MegaApiClient;
using SPTInstaller.Helpers;
using System.Threading.Tasks;
using Serilog;

namespace SPTInstaller.Models.Mirrors.Downloaders;
public class MegaMirrorDownloader : MirrorDownloaderBase
{
    public MegaMirrorDownloader(DownloadMirror mirrorInfo) : base(mirrorInfo)
    {
    }

    public override async Task<FileInfo?> Download(IProgress<double> progress)
    {
        var megaClient = new MegaApiClient();
        await megaClient.LoginAnonymousAsync();

        // if mega fails to connect, just return
        if (!megaClient.IsLoggedIn)
            return null;

        try
        {
            var file = new FileInfo(Path.Join(DownloadCacheHelper.CachePath, "patcher"));
            
            await megaClient.DownloadFileAsync(new Uri(MirrorInfo.Link),
                file.FullName, progress);
            
            file.Refresh();

            if (!file.Exists)
                return null;

            return FileHashHelper.CheckHash(file, MirrorInfo.Hash) ? file : null;
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Exception thrown while downloading from mega");
            //most likely a 509 (Bandwidth limit exceeded) due to mega's user quotas.
            return null;
        }
    }
}
