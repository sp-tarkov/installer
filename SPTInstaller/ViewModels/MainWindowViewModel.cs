using Avalonia;
using ReactiveUI;
using Serilog;
using System.Globalization;
using System.Reflection;

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
    
    public MainWindowViewModel(bool debugging)
    {
        Title =
            $"{(debugging ? "-debug-" : "")} SPT Installer {"v" + Assembly.GetExecutingAssembly().GetName()?.Version?.ToString() ?? "--unknown version--"}";
        
        Log.Information($"========= {Title} Started =========");
        Log.Information(Environment.OSVersion.VersionString);
        
        var uiCulture = CultureInfo.InstalledUICulture;
        
        Log.Information("System Language: {iso} - {name}", uiCulture.TwoLetterISOLanguageName, uiCulture.DisplayName);
        
        Router.Navigate.Execute(new InstallerUpdateViewModel(this, debugging));
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