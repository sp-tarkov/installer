using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ReactiveUI;
using SPTInstaller.Helpers;
using SPTInstaller.Models;

namespace SPTInstaller.ViewModels;

public class InstallPathSelectionViewModel : ViewModelBase
{
    private bool _debugging = false;
    private InternalData _data;
    
    private string _selectedPath;
    
    public string SelectedPath
    {
        get => _selectedPath;
        set => this.RaiseAndSetIfChanged(ref _selectedPath, value);
    }
    
    private bool _validPath;
    public bool ValidPath
    {
        get => _validPath;
        set => this.RaiseAndSetIfChanged(ref _validPath, value);
    }
    
    private string _errorMessage;
    
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }
    
    public InstallPathSelectionViewModel(IScreen host, bool debugging) : base(host)
    {
        _debugging = debugging;
        _data = ServiceHelper.Get<InternalData?>() ?? throw new Exception("Failed to get internal data");
        SelectedPath = Environment.CurrentDirectory;
        ValidPath = false;
        
        AdjustInstallPath();
    }
    
    public async Task SelectFolderCommand()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow == null)
            {
                return;
            }
            
            var startingFolderPath = Directory.Exists(SelectedPath) ? SelectedPath : Environment.CurrentDirectory;
            
            var suggestedFolder = await desktop.MainWindow.StorageProvider.TryGetFolderFromPathAsync(startingFolderPath);
                
            var selections = await desktop.MainWindow.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions()
                {
                    AllowMultiple = false,
                    SuggestedStartLocation = suggestedFolder, 
                    Title = "Select a folder to install SPT into"
                });
            
            SelectedPath = selections.First().Path.LocalPath;
        } 
    }
    
    public void ValidatePath()
    {
        if (String.IsNullOrEmpty(SelectedPath))
        {
            ErrorMessage = "Please provide an install path";
            ValidPath = false;
            return;
        }
        
        var match = Regex.Match(SelectedPath[2..], @"[\/:*?""<>|]");
        if (match.Success)
        {
            ErrorMessage = "Path cannot contain these characters: / : * ? \" < > |";
            ValidPath = false;
            return;
        }
        
        if (FileHelper.CheckPathForProblemLocations(SelectedPath, out var failedCheck))
        {
            if (failedCheck.CheckType == PathCheckType.EndsWith)
            {
                ErrorMessage = $"You can install in {failedCheck.Target}, but only in a subdirectory. Example: ..\\{failedCheck.Target}\\SPT";
                ValidPath = false;
                return;
            }
            
            if (failedCheck.CheckAction == PathCheckAction.Deny)
            {
                ErrorMessage = $"Sorry, you cannot install in {failedCheck.Target}";
                ValidPath = false;
                return;
            }
        }
        
        ValidPath = true;
    }
    
    private void AdjustInstallPath()
    {
        if (FileHelper.CheckPathForProblemLocations(SelectedPath, out var failedCheck))
        {
            switch (failedCheck.CheckType)
            {
                case PathCheckType.EndsWith:
                    SelectedPath = Path.Join(Environment.CurrentDirectory, "SPT");
                    break;
                
                case PathCheckType.Contains:
                case PathCheckType.DriveRoot:
                    SelectedPath = Path.Join(Directory.GetDirectoryRoot(Environment.CurrentDirectory), "SPT");
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    public async Task NextCommand()
    {
        if (FileHelper.CheckPathForProblemLocations(SelectedPath, out var failedCheck) && failedCheck.CheckAction == PathCheckAction.Deny)
        {
            return;
        }
        
        _data.TargetInstallPath = SelectedPath;
        
        NavigateTo(new PreChecksViewModel(HostScreen, _debugging));
    }
}