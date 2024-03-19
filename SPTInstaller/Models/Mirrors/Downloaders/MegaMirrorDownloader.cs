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
            using var megaDownloadStream = await megaClient.DownloadAsync(new Uri(MirrorInfo.Link), progress);

            var file = await DownloadCacheHelper.DownloadFileAsync("patcher.zip", megaDownloadStream);

            if (file == null)
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
