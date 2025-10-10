using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using ReactiveUI;
using Serilog;
using SPTInstaller.CustomControls;
using SPTInstaller.Helpers;
using SPTInstaller.Interfaces;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using SPTInstaller.Models;

namespace SPTInstaller.ViewModels;

public class MessageViewModel : ViewModelBase
{
    private bool _HasErrors;
    
    public bool HasErrors
    {
        get => _HasErrors;
        set => this.RaiseAndSetIfChanged(ref _HasErrors, value);
    }
    
    private string _Message;
    
    public string Message
    {
        get => _Message;
        set => this.RaiseAndSetIfChanged(ref _Message, value);
    }
    
    private bool _showCloseButton;
    
    public bool ShowCloseButton
    {
        get => _showCloseButton;
        set => this.RaiseAndSetIfChanged(ref _showCloseButton, value);
    }
    
    private bool _showOptions;
    
    public bool ShowOptions
    {
        get => _showOptions;
        set => this.RaiseAndSetIfChanged(ref _showOptions, value);
    }
    
    private string _cacheInfoText;
    
    public string CacheInfoText
    {
        get => _cacheInfoText;
        set => this.RaiseAndSetIfChanged(ref _cacheInfoText, value);
    }
    
    private string _clipCommandText;
    
    public string ClipCommandText
    {
        get => _clipCommandText;
        set => this.RaiseAndSetIfChanged(ref _clipCommandText, value);
    }
    
    private bool _addShortcuts;
    public bool AddShortcuts
    {
        get => _addShortcuts;
        set => this.RaiseAndSetIfChanged(ref _addShortcuts, value);
    }
    
    private bool _openInstallFolder = true;
    public bool OpenInstallFolder
    {
        get => _openInstallFolder;
        set => this.RaiseAndSetIfChanged(ref _openInstallFolder, value);
    }
    
    public ICommand CopyLogFileToClipboard => ReactiveCommand.CreateFromTask(async () =>
    {
        var data = ServiceHelper.Get<InternalData>();
        
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                if (desktop.MainWindow?.Clipboard == null)
                {
                    ClipCommandText = "Could not get clipboard :(";
                    return;
                }
                
                var dataObject = new DataObject();

                var filesToCopy = new List<IStorageFile>();
                
                var logFile = await desktop.MainWindow.StorageProvider.TryGetFileFromPathAsync(data.DebugMode ? App.LogDebugPath : App.LogPath);
                var patcherLogFile = await desktop.MainWindow.StorageProvider.TryGetFileFromPathAsync(Path.Join(data.TargetInstallPath, "patcher.log"));
                    
                if (logFile == null)
                {
                    ClipCommandText = "Could not get log file :(";
                    return;
                }
                
                filesToCopy.Add(logFile);

                if (patcherLogFile != null)
                {
                    filesToCopy.Add(patcherLogFile);
                }
                
                dataObject.Set(DataFormats.Files, filesToCopy.ToArray());
                
                await desktop.MainWindow.Clipboard.SetDataObjectAsync(dataObject);
                ClipCommandText = "Copied!";
            }
            catch (Exception ex)
            {
                ClipCommandText = ex.Message;
            }
        } 
    });
    
    private StatusSpinner.SpinnerState _cacheCheckState;
    
    public StatusSpinner.SpinnerState CacheCheckState
    {
        get => _cacheCheckState;
        set => this.RaiseAndSetIfChanged(ref _cacheCheckState, value);
    }
    
    public ICommand CloseCommand { get; set; } = ReactiveCommand.Create(() =>
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.MainWindow.Close();
        }
    });
    
    public MessageViewModel(IScreen Host, IResult result, bool showCloseButton = true, bool noLog = false) : base(Host)
    {
        ShowCloseButton = showCloseButton;
        Message = result.Message;
        ClipCommandText = "Copy installer log to clipboard";
        
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            var data = ServiceHelper.Get<InternalData?>();
            
            desktopApp.MainWindow.Closing += (_, _) =>
            {
                if (ShowOptions)
                {
                    if (OpenInstallFolder)
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                           FileName = "explorer.exe",
                           Arguments = data.TargetInstallPath
                        });
                    }
                    
                    if (AddShortcuts)
                    {
                        Log.Information("Running add shortcuts script ...");
                        var sptPath = $"{Path.Join(data.TargetInstallPath, "SPT")}";
                        var shortcutResult = ProcessHelper.RunEmbeddedScript("desktop_shortcut.ps1", sptPath);
                        if (!shortcutResult.Succeeded)
                        {
                            Log.Fatal(shortcutResult.Message);
                            return;
                        }
                    }
                }
                
                try
                {
                    if (data.TargetInstallPath == Environment.CurrentDirectory)
                    {
                        return;
                    }
                    
                    File.Copy(App.LogPath, Path.Join(data.TargetInstallPath, "spt-installer.log"), true);

                    if (data.DebugMode)
                    {
                        File.Copy(App.LogDebugPath, Path.Join(data.TargetInstallPath, "spt-installer-debug.log"), true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to copy installer log to install path");
                }
            };
        }
        
        Task.Run(() =>
        {
            CacheInfoText = "Getting cache size ...";
            CacheCheckState = StatusSpinner.SpinnerState.Running;
            
            CacheInfoText = $"Cache Size: {DownloadCacheHelper.GetCacheSizeText()}";
            CacheCheckState = StatusSpinner.SpinnerState.OK;
        });
        
        if (result.Succeeded)
        {
            Log.Information(Message);
            ShowOptions = true;
            return;
        }
        
        HasErrors = true;
        
        if (!noLog)
            Log.Error(Message);
    }
}