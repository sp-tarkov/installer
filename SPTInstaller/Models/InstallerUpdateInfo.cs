using Gitea.Api;
using Gitea.Client;
using ReactiveUI;
using Serilog;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SPTInstaller.Models;
public class InstallerUpdateInfo : ReactiveObject
{
    private bool _updateAvailable;
    public bool UpdateAvailable
    {
        get => _updateAvailable;
        set => this.RaiseAndSetIfChanged(ref _updateAvailable, value);
    }

    private Version _currentVersion;
    public Version CurrentVersion
    {
        get => _currentVersion;
        set => this.RaiseAndSetIfChanged(ref _currentVersion, value);
    }

    private Version _newVersion;
    public Version NewVersion
    {
        get => _newVersion;
        set => this.RaiseAndSetIfChanged(ref _newVersion, value);
    }


    private bool _checkingForUpdates;
    public bool CheckingForUpdates
    {
        get => _checkingForUpdates;
        set => this.RaiseAndSetIfChanged(ref _checkingForUpdates, value);
    }

    public async Task<bool> CheckForUpdates()
    {
        CheckingForUpdates = true;

        try
        {
            var repo = new RepositoryApi(Configuration.Default);

            var releases = await repo.RepoListReleasesAsync("CWX", "SPT-AKI-Installer");

            if (releases == null || releases.Count == 0)
                return false;

            var latest = releases.FindAll(x => !x.Prerelease)[0];

            if (latest == null)
                return false;

            var latestVersion = new Version(latest.TagName);

            if (latestVersion == null || latestVersion <= CurrentVersion)
                return false;

            NewVersion = latestVersion;
            UpdateAvailable = true;
            CheckingForUpdates = false;

            return true;
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Failed to check for updates");
        }

        CheckingForUpdates = false;
        return false;
    }

    public ICommand UpdateInstaller { get; set; }

    public InstallerUpdateInfo(Version? currentVersion)
    {
        if (currentVersion == null)
            return;

        CurrentVersion = currentVersion;

        UpdateInstaller = ReactiveCommand.Create(() =>
        {
            // TODO: update installer here
        });
    }
}
