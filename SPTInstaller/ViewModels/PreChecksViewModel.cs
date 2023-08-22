using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using DialogHostAvalonia;
using ReactiveUI;
using Serilog;
using SPTInstaller.Controllers;
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
        }
#endif

        data.TargetInstallPath = Environment.CurrentDirectory;
        InstallPath = data.TargetInstallPath;

        Log.Information($"Install Path: {FileHelper.GetRedactedPath(InstallPath)}");

        StartInstallCommand = ReactiveCommand.Create(async () =>
        {
            if(FileHelper.CheckPathForProblemLocations(InstallPath))
            {
                var confirmation = await DialogHost.Show(new ConfirmationDialog($"We suspect you may be installing to your desktop or a cloud synced folder.\nYou might want to considering installing somewhere else to avoid issues.\n\nAre you sure you want to install to this path?\n{InstallPath}"));

                if (confirmation == null || !bool.TryParse(confirmation.ToString(), out var confirm) || !confirm)
                {
                    return;
                }
            }

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
    }
}