using Avalonia.Controls;
using SPTInstaller.Helpers;
using System.Diagnostics;

namespace SPTInstaller.CustomControls.Dialogs;
public partial class WhyCacheThoughDialog : UserControl
{
    public WhyCacheThoughDialog()
    {
        InitializeComponent();
    }

    public bool CacheExists => Directory.Exists(DownloadCacheHelper.CachePath);

    public void OpenCacheFolder()
    {
        if (!CacheExists)
            return;

        Process.Start(new ProcessStartInfo()
        {
            FileName = Path.EndsInDirectorySeparator(DownloadCacheHelper.CachePath) ? DownloadCacheHelper.CachePath : DownloadCacheHelper.CachePath + Path.DirectorySeparatorChar,
            UseShellExecute = true,
            Verb = "open"
        });
    }
}
