using Avalonia;
using Gitea.Client;
using ReactiveUI;
using Serilog;
using SPTInstaller.Models;
using System.Reflection;
using System.Threading.Tasks;

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
        Configuration.Default.BasePath = "https://dev.sp-tarkov.com/api/v1";

        Version? version = Assembly.GetExecutingAssembly().GetName()?.Version;

        Title = $"SPT Installer {"v" + version?.ToString() ?? "--unknown version--"}";

        Log.Information($"========= {Title} Started =========");
        Log.Information(Environment.OSVersion.VersionString);

        var updateInfo = new InstallerUpdateInfo(version);

        Task.Run(async () =>
        {
            await updateInfo.CheckForUpdates();
        });

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