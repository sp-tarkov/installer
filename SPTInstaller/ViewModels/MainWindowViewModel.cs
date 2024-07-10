using Avalonia;
using ReactiveUI;
using Serilog;
using System.Globalization;
using System.Reflection;
using SPTInstaller.Helpers;
using SPTInstaller.Models;

namespace SPTInstaller.ViewModels;

public class MainWindowViewModel : ReactiveObject, IActivatableViewModel, IScreen
{
    public RoutingState Router { get; } = new();
    public ViewModelActivator Activator { get; } = new();
    
    private string _title;
    
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    
    public MainWindowViewModel(string installPath)
    {
        var data = ServiceHelper.Get<InternalData>() ?? throw new Exception("failed to get interanl data");
        
        Title =
            $"{(data.DebugMode ? "-debug-" : "")} SPT Installer {"v" + Assembly.GetExecutingAssembly().GetName()?.Version?.ToString() ?? "--unknown version--"}";
        
        Log.Information($"========= {Title} Started =========");
        Log.Information(Environment.OSVersion.VersionString);
        
        var uiCulture = CultureInfo.InstalledUICulture;
        
        Log.Information("System Language: {iso} - {name}", uiCulture.TwoLetterISOLanguageName, uiCulture.DisplayName);
        
        Router.Navigate.Execute(new InstallerUpdateViewModel(this, installPath));
    }
    
    public void CloseCommand()
    {
        if (Application.Current.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.MainWindow.Close();
        }
    }
    
    public void MinimizeCommand()
    {
        if (Application.Current.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.MainWindow.WindowState = Avalonia.Controls.WindowState.Minimized;
        }
    }
}