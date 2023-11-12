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

    private bool _showCard = false;
    public bool ShowCard
    {
        get => _showCard;
        set => this.RaiseAndSetIfChanged(ref _showCard, value);
    }

    private bool _updating = false;
    public bool Updating
    {
        get => _updating;
        set => this.RaiseAndSetIfChanged(ref _updating, value);
    }

    private bool _updateAvailable = false;
    public bool UpdateAvailable
    {
        get => _updateAvailable;
        set => this.RaiseAndSetIfChanged(ref _updateAvailable, value);
    }

    private bool _checkingForUpdates = false;
    public bool CheckingForUpdates
    {
        get => _checkingForUpdates;
        set => this.RaiseAndSetIfChanged(ref _checkingForUpdates, value);
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
        UpdateAvailable = false;
        
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

        var file = await DownloadCacheHelper.DownloadFileAsync("SPTInstller.exe", NewInstallerUrl, progress);

        if (file == null || !file.Exists)
        {
            UpdateInfoText = "Failed to download new installer :(";
            return "";
        }

        return file.FullName;
    }

    private void EndCheck(string infoText, bool updateAvailable, bool log = true)
    {
        if (log)
        {
            Log.Information(infoText);
        }
        
        UpdateInfoText = infoText;

        if (!updateAvailable)
        {
            Task.Run(async () =>
            {
                // delay card dismiss
                await Task.Delay(TimeSpan.FromSeconds(2));
                ShowCard = updateAvailable;
            });
        }
        else
        {
            ShowCard = updateAvailable;
        }

        CheckingForUpdates = false;
        UpdateAvailable = updateAvailable;
    }

    public async Task CheckForUpdates(Version? currentVersion)
    {
        if (currentVersion == null)
            return;

        UpdateInfoText = "Checking for installer updates";
        ShowCard = true;
        CheckingForUpdates = true;

        try
        {
            var repo = new RepositoryApi(Configuration.Default);

            var releases = await repo.RepoListReleasesAsync("CWX", "SPT-AKI-Installer");

            if (releases == null || releases.Count == 0)
            {
                EndCheck("No releases available", false);
                return;
            }

            var latest = releases.FindAll(x => !x.Prerelease)[0];

            if (latest == null)
            {
                EndCheck("could not get latest release", false);
                return;
            }

            var latestVersion = new Version(latest.TagName);

            if (latestVersion == null || latestVersion <= currentVersion)
            {
                EndCheck("No updates available", false);
                return;
            }

            _newVersion = latestVersion;

            NewInstallerUrl = latest.Assets[0].BrowserDownloadUrl;

            EndCheck($"Update available, version {latestVersion}", true);

            return;
        }
        catch (Exception ex)
        {
            EndCheck(ex.Message, false, false);
            Log.Error(ex, "Failed to check for updates");
        }

        return;
    }
}
