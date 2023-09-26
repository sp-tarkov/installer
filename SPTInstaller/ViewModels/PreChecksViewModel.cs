using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using DialogHostAvalonia;
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

    public InstallerUpdateInfo UpdateInfo { get; set; } = new InstallerUpdateInfo();

    private string _installPath;
    public string InstallPath
    {
        get => _installPath;
        set => this.RaiseAndSetIfChanged(ref _installPath, value);
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

    public PreChecksViewModel(IScreen host) : base(host)
    {
        var data = ServiceHelper.Get<InternalData?>();
        var installer = ServiceHelper.Get<InstallController?>();

        if (data == null || installer == null)
        {
            NavigateTo(new MessageViewModel(HostScreen, Result.FromError("Failed to get required service for prechecks")));
            return;
        }

        data.OriginalGamePath = PreCheckHelper.DetectOriginalGamePath();

#if !TEST
        if (data.OriginalGamePath == null)
        {
            NavigateTo(new MessageViewModel(HostScreen, Result.FromError("Could not find EFT install.\n\nDo you own and have the game installed?")));
            return;
        }
#endif

        data.TargetInstallPath = Environment.CurrentDirectory;
        InstallPath = data.TargetInstallPath;

        Log.Information($"Install Path: {FileHelper.GetRedactedPath(InstallPath)}");

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

        StartInstallCommand = ReactiveCommand.Create(async () =>
        {
            UpdateInfo.ShowCard = false;
            NavigateTo(new InstallViewModel(HostScreen));
        });

        ShowDetailedViewCommand = ReactiveCommand.Create(() => 
        {
            UpdateInfo.ShowCard = false;
            Log.Logger.Information("Opening Detailed PreCheck View");
            NavigateTo(new DetailedPreChecksViewModel(HostScreen));
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
            var result = await installer.RunPreChecks();

            await UpdateInfo.CheckForUpdates(Assembly.GetExecutingAssembly().GetName()?.Version);

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