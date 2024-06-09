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
    
    public ICommand CopyLogFileToClipboard => ReactiveCommand.CreateFromTask(async () =>
    {
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
                var logFile = await desktop.MainWindow.StorageProvider.TryGetFileFromPathAsync(App.LogPath);
                
                if (logFile == null)
                {
                    ClipCommandText = "Could not get log file :(";
                    return;
                }
                
                dataObject.Set(DataFormats.Files, new[] {logFile});
                
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
        if (Application.Current.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.MainWindow.Close();
        }
    });
    
    public MessageViewModel(IScreen Host, IResult result, bool showCloseButton = true, bool noLog = false) : base(Host)
    {
        ShowCloseButton = showCloseButton;
        Message = result.Message;
        ClipCommandText = "Copy installer log to clipboard";
        
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
            return;
        }
        
        HasErrors = true;
        
        if (!noLog)
            Log.Error(Message);
    }
}