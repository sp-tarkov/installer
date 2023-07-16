using Avalonia;
using ReactiveUI;
using Serilog;
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

    public MainWindowViewModel()
    {
        string? version = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();

        Title = $"SPT Installer {"v" + version ?? "--unknown version--"}";

        Log.Information($"========= {Title} Started =========");
        Log.Information(Environment.OSVersion.VersionString);

        Router.Navigate.Execute(new PreChecksViewModel(this));
    }

    public void CloseCommand()
    {
        if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.MainWindow.Close();
        }
    }

    public void MinimizeCommand()
    {
        if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.MainWindow.WindowState = Avalonia.Controls.WindowState.Minimized;
        }
    }

}