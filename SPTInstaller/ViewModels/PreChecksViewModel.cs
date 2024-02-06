using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using DialogHostAvalonia;
using Gitea.Api;
using Gitea.Client;
using ReactiveUI;
using Serilog;
using SPTInstaller.Controllers;
using SPTInstaller.CustomControls;
using SPTInstaller.CustomControls.Dialogs;
using SPTInstaller.Helpers;
using SPTInstaller.Models;

namespace SPTInstaller.ViewModels;

public class PreChecksViewModel : ViewModelBase
{
    public ObservableCollection<PreCheckBase> PreChecks { get; set; } = new(ServiceHelper.GetAll<PreCheckBase>());
    public ICommand StartInstallCommand { get; set; }
    public ICommand ShowDetailedViewCommand { get; set; }

    public ICommand UpdateInstallerCommand { get; set; }

    public ICommand DismissUpdateCommand { get; set; }
    
    public ICommand LaunchWithDebug { get; set; }

    public InstallerUpdateInfo UpdateInfo { get; set; } = new();

    private bool _debugging;

    public bool Debugging
    {
        get => _debugging;
        set => this.RaiseAndSetIfChanged(ref _debugging, value);
    }

    private string _installPath;
    public string InstallPath
    {
        get => _installPath;
        set => this.RaiseAndSetIfChanged(ref _installPath, value);
    }

    private string _installButtonText;

    public string InstallButtonText
    {
        get => _installButtonText;
        set => this.RaiseAndSetIfChanged(ref _installButtonText, value);
    }
    
    private bool _allowInstall;
    public bool AllowInstall
    {
        get => _allowInstall;
        set => this.RaiseAndSetIfChanged(ref _allowInstall, value);
    }

    private bool _allowDetailsButton = false; 
    public bool AllowDetailsButton
    {
        get => _allowDetailsButton;
        set => this.RaiseAndSetIfChanged(ref _allowDetailsButton, value);
    }

    private string _cacheInfoText;
    public string CacheInfoText
    {
        get => _cacheInfoText;
        set => this.RaiseAndSetIfChanged(ref _cacheInfoText, value);
    }

    private StatusSpinner.SpinnerState _cacheCheckState;
    public StatusSpinner.SpinnerState CacheCheckState
    {
        get => _cacheCheckState;
        set => this.RaiseAndSetIfChanged(ref _cacheCheckState, value);
    }
    
    private StatusSpinner.SpinnerState _installButtonCheckState;
    public StatusSpinner.SpinnerState InstallButtonCheckState
    {
        get => _installButtonCheckState;
        set => this.RaiseAndSetIfChanged(ref _installButtonCheckState, value);
    }

    private void ReCheckRequested(object? sender, EventArgs e)
    {
        Task.Run(async () =>
        {
            if (sender is InstallController installer)
            {
                var result = await installer.RunPreChecks();
                AllowInstall = result.Succeeded;
            }
        });
    }

    public PreChecksViewModel(IScreen host, bool debugging) : base(host)
    {
        Debugging = debugging;
        var data = ServiceHelper.Get<InternalData?>();
        var installer = ServiceHelper.Get<InstallController?>();

        installer.RecheckRequested += ReCheckRequested;

        InstallButtonText = "Please wait ...";
        InstallButtonCheckState = StatusSpinner.SpinnerState.Pending;

        if (data == null || installer == null)
        {
            NavigateTo(new MessageViewModel(HostScreen, Result.FromError("Failed to get required service for prechecks")));
            return;
        }

        data.OriginalGamePath = PreCheckHelper.DetectOriginalGamePath();
        
        data.TargetInstallPath = Environment.CurrentDirectory;
        InstallPath = data.TargetInstallPath;

        Log.Information($"Install Path: {FileHelper.GetRedactedPath(InstallPath)}");

#if !TEST
        if (data.OriginalGamePath == null)
        {
            NavigateTo(new MessageViewModel(HostScreen, Result.FromError("Could not find EFT install.\n\nDo you own and have the game installed?")));
            return;
        }
#endif

        if (data.OriginalGamePath == data.TargetInstallPath)
        {
            Log.CloseAndFlush();

            var logFiles = Directory.GetFiles(InstallPath, "spt-aki-installer_*.log");

            // remove log file from original game path if they exist
            foreach (var file in logFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            NavigateTo(new MessageViewModel(HostScreen, Result.FromError("Installer is located in EFT's original directory. Please move the installer to a seperate folder as per the guide"), noLog: true));
            return;
        }

        Task.Run(async () =>
        {
            if (FileHelper.CheckPathForProblemLocations(InstallPath, out var detectedName))
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    Log.Warning("Problem folder detected, confirming install path ...");
                    var confirmation = await DialogHost.Show(new ConfirmationDialog($"We suspect you may be installing into a problematic folder: {detectedName}.\nYou might want to consider installing somewhere else to avoid issues.\n\nAre you sure you want to install to this path?\n{InstallPath}"));

                    if (confirmation == null || !bool.TryParse(confirmation.ToString(), out var confirm) || !confirm)
                    {
                        Log.Information("User declined install path, exiting");
                        Environment.Exit(0);
                    }
                });

                Log.Information("User accepted install path");
            }
        });

        LaunchWithDebug = ReactiveCommand.Create(async () =>
        {
            try
            {
                var installerPath = Path.Join(_installPath, "SPTInstaller.exe");
                Process.Start(new ProcessStartInfo()
                {
                    FileName = installerPath,
                    Arguments = "debug"
                });

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to enter debug mode");
            }
        });

        StartInstallCommand = ReactiveCommand.Create(async () =>
        {
            UpdateInfo.ShowCard = false;
            NavigateTo(new InstallViewModel(HostScreen));
        });

        ShowDetailedViewCommand = ReactiveCommand.Create(() => 
        {
            UpdateInfo.ShowCard = false;
            Log.Logger.Information("Opening Detailed PreCheck View");
            installer.RecheckRequested -= ReCheckRequested;
            NavigateTo(new DetailedPreChecksViewModel(HostScreen, Debugging));
        });

        UpdateInstallerCommand = ReactiveCommand.Create(async () =>
        {
            AllowDetailsButton = false;
            AllowInstall = false;
            await UpdateInfo.UpdateInstaller();
        });

        DismissUpdateCommand = ReactiveCommand.Create(() =>
        {
            UpdateInfo.ShowCard = false;
        });


        Task.Run(async () =>
        {
            // run prechecks
            var result = await installer.RunPreChecks();

            // check for updates
            await UpdateInfo.CheckForUpdates(Assembly.GetExecutingAssembly().GetName()?.Version);
            
            // get latest spt version
            InstallButtonText = "Getting latest release ...";
            InstallButtonCheckState = StatusSpinner.SpinnerState.Running;
            
            var repo = new RepositoryApi(Configuration.Default);
            var akiRepoReleases = await repo.RepoListReleasesAsync("SPT-AKI", "Stable-releases");

            if (akiRepoReleases == null || akiRepoReleases.Count == 0)
            {
                InstallButtonText = "Could not get SPT releases from repo";
                InstallButtonCheckState = StatusSpinner.SpinnerState.Error;
                return;
            }
            
            var latestAkiRelease = akiRepoReleases.FindAll(x => !x.Prerelease)[0];

            if (latestAkiRelease == null)
            {
                InstallButtonText = "Could not find the latest SPT release";
                InstallButtonCheckState = StatusSpinner.SpinnerState.Error;
                return;
            }
            
            InstallButtonText = $"Start Install: SPT v{latestAkiRelease.TagName}";
            InstallButtonCheckState = StatusSpinner.SpinnerState.OK;
            
            AllowDetailsButton = true;
            AllowInstall = result.Succeeded;
        });

        Task.Run(() =>
        {
            CacheInfoText = "Getting cache size ...";
            CacheCheckState = StatusSpinner.SpinnerState.Running;

            CacheInfoText = $"Cache Size: {DownloadCacheHelper.GetCacheSizeText()}";
            CacheCheckState = StatusSpinner.SpinnerState.OK;
        });
    }
}