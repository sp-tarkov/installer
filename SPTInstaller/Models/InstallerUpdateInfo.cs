using Gitea.Api;
using Gitea.Client;
using ReactiveUI;
using Serilog;
using SPTInstaller.Helpers;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SPTInstaller.Models;
public class InstallerUpdateInfo : ReactiveObject
{
    private Version? _newVersion;

    public string NewInstallerUrl = "";

    private string _updateInfoText = "";
    public string UpdateInfoText
    {
        get => _updateInfoText;
        set => this.RaiseAndSetIfChanged(ref _updateInfoText, value);
    }

    private bool _updateAvailable;
    public bool UpdateAvailable
    {
        get => _updateAvailable;
        set => this.RaiseAndSetIfChanged(ref _updateAvailable, value);
    }

    private bool _updating;
    public bool Updating
    {
        get => _updating;
        set => this.RaiseAndSetIfChanged(ref _updating, value);
    }

    private int _downloadProgress;
    public int DownloadProgress
    {
        get => _downloadProgress;
        set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
    }

    public async Task UpdateInstaller()
    {
        Updating = true;
        
        var updater = new FileInfo(Path.Join(DownloadCacheHelper.CachePath, "update.ps1"));
        FileHelper.StreamAssemblyResourceOut("update.ps1", updater.FullName);


        if (!updater.Exists)
        {
            UpdateInfoText = "Failed to get updater from resources :(";
            return;
        }

        var newInstallerPath = await DownloadNewInstaller();

        if(string.IsNullOrWhiteSpace(newInstallerPath))
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            ArgumentList = { "-ExecutionPolicy", "Bypass", "-File", $"{updater.FullName}", $"{newInstallerPath}", $"{Path.Join(Environment.CurrentDirectory, "SPTInstaller.exe")}" }
        });
    }

    private async Task<string> DownloadNewInstaller()
    {
        UpdateInfoText = $"Downloading new installer v{_newVersion}";

        var progress = new Progress<double>(x => DownloadProgress = (int)x);

        var file = await DownloadCacheHelper.GetOrDownloadFileAsync("SPTInstller.exe", NewInstallerUrl, progress);

        if (file == null || !file.Exists)
        {
            UpdateInfoText = "Failed to download new installer :(";
            return "";
        }

        return file.FullName;
    }

    public async Task CheckForUpdates(Version? currentVersion)
    {
        if (currentVersion == null)
            return;

        try
        {
            var repo = new RepositoryApi(Configuration.Default);

            var releases = await repo.RepoListReleasesAsync("CWX", "SPT-AKI-Installer");

            if (releases == null || releases.Count == 0)
                return;

            var latest = releases.FindAll(x => !x.Prerelease)[0];

            if (latest == null)
                return;

            var latestVersion = new Version(latest.TagName);

            if (latestVersion == null || latestVersion <= currentVersion)
                return;

            UpdateAvailable = true;

            _newVersion = latestVersion;

            UpdateInfoText = $"A newer installer is available, version {latestVersion}";

            NewInstallerUrl = latest.Assets[0].BrowserDownloadUrl;

            return;
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Failed to check for updates");
        }

        return;
    }
}
