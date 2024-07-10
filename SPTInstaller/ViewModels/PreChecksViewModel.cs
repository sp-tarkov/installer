using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using DialogHostAvalonia;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using SPTInstaller.Controllers;
using SPTInstaller.CustomControls;
using SPTInstaller.CustomControls.Dialogs;
using SPTInstaller.Helpers;
using SPTInstaller.Models;
using SPTInstaller.Models.ReleaseInfo;

namespace SPTInstaller.ViewModels;

public class PreChecksViewModel : ViewModelBase
{
    private bool _hasPreCheckSelected;
    
    public bool HasPreCheckSelected
    {
        get => _hasPreCheckSelected;
        set => this.RaiseAndSetIfChanged(ref _hasPreCheckSelected, value);
    }
    
    public ObservableCollection<PreCheckBase> PreChecks { get; set; } = new(ServiceHelper.GetAll<PreCheckBase>());
    
    public ICommand SelectPreCheckCommand { get; set; }
    public ICommand StartInstallCommand { get; set; }
    
    public ICommand LaunchWithDebug { get; set; }
    
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
    
    public PreChecksViewModel(IScreen host) : base(host)
    {
        var data = ServiceHelper.Get<InternalData?>();
        var installer = ServiceHelper.Get<InstallController?>();
        
        Debugging = data.DebugMode;
        
        installer.RecheckRequested += ReCheckRequested;
        
        InstallButtonText = "Please wait ...";
        InstallButtonCheckState = StatusSpinner.SpinnerState.Pending;
        
        if (data == null || installer == null)
        {
            NavigateTo(new MessageViewModel(HostScreen,
                Result.FromError("Failed to get required service for prechecks")));
            return;
        }
        
        data.OriginalGamePath = PreCheckHelper.DetectOriginalGamePath();
        
        InstallPath = data.TargetInstallPath;
        
        Log.Information($"Install Path: {FileHelper.GetRedactedPath(InstallPath)}");
        
#if !TEST
        if (data.OriginalGamePath == null)
        {
            NavigateTo(new MessageViewModel(HostScreen,
                Result.FromError("Could not find where you installed EFT.\n\nDo you own and have the game installed?")));
            return;
        }
#endif
        
        if (data.OriginalGamePath == data.TargetInstallPath)
        {
            Log.CloseAndFlush();
            
            var logFiles = Directory.GetFiles(InstallPath, "spt-installer_*.log");
            
            // remove log file from original game path if they exist
            foreach (var file in logFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }
            
            NavigateTo(new MessageViewModel(HostScreen,
                Result.FromError(
                    "You have chosen to install in the same folder as EFT. Please choose a another folder. Refer to the install guide on where best to place the installer before running it."),
                noLog: true));
            return;
        }
        
        Task.Run(async () =>
        {
            if (FileHelper.CheckPathForProblemLocations(InstallPath, out var failedCheck))
            {
                switch (failedCheck.CheckAction)
                {
                    case PathCheckAction.Warn:
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            Log.Warning("Problem path detected, confirming install path ...");
                            var confirmation = await DialogHost.Show(new ConfirmationDialog(
                                $"It appears you are installing into a folder known to cause problems: {failedCheck.Target}." +
                                $"\nPlease consider installing SPT somewhere else to avoid issues later on." +
                                $"\n\nAre you sure you want to install to this path?\n{InstallPath}"));
                            
                            if (confirmation == null || !bool.TryParse(confirmation.ToString(), out var confirm) ||
                                !confirm)
                            {
                                Log.Information("User declined install path");
                                NavigateBack();
                            }
                        });
                        
                        break;
                    }
                    
                    case PathCheckAction.Deny:
                    {
                        Log.Error("Problem path detected, install denied");
                        NavigateTo(new MessageViewModel(HostScreen,
                            Result.FromError(
                                $"We suspect you may be installing into a problematic folder: {failedCheck.Target}.\nWe won't be letting you install here. How did you do this?")));
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                Log.Information("User accepted install path");
            }
        });
        
        LaunchWithDebug = ReactiveCommand.Create(async () =>
        {
            try
            {
                App.ReLaunch(true, InstallPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to enter debug mode");
            }
        });
        
        SelectPreCheckCommand = ReactiveCommand.Create(async (PreCheckBase check) =>
        {
            foreach (var precheck in PreChecks)
            {
                if (check.Id == precheck.Id)
                {
                    precheck.IsSelected = true;
                    
                    HasPreCheckSelected = true;
                    
                    continue;
                }
                
                precheck.IsSelected = false;
            }
        });
        
        StartInstallCommand = ReactiveCommand.Create(async () =>
        {
            NavigateTo(new InstallViewModel(HostScreen));
        });
        
        Task.Run(async () =>
        {
            // run prechecks
            var result = await installer.RunPreChecks();
            
            // get latest spt version
            InstallButtonText = "Getting latest release ...";
            InstallButtonCheckState = StatusSpinner.SpinnerState.Running;
            
            var progress = new Progress<double>((d) => { });
            
            var SPTReleaseInfoFile =
                await DownloadCacheHelper.GetOrDownloadFileAsync("release.json", DownloadCacheHelper.ReleaseMirrorUrl,
                    progress, DownloadCacheHelper.SuggestedTtl);
            
            if (SPTReleaseInfoFile == null)
            {
                InstallButtonText = "Could not get SPT release metadata";
                InstallButtonCheckState = StatusSpinner.SpinnerState.Error;
                return;
            }
            
            var SPTReleaseInfo =
                JsonConvert.DeserializeObject<ReleaseInfo>(File.ReadAllText(SPTReleaseInfoFile.FullName));
            
            if (SPTReleaseInfo == null)
            {
                InstallButtonText = "Could not parse latest SPT release";
                InstallButtonCheckState = StatusSpinner.SpinnerState.Error;
                return;
            }
            
            InstallButtonText = $"Start Install: SPT v{SPTReleaseInfo.SPTVersion}";
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