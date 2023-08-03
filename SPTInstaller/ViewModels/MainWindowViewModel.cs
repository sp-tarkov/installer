using Avalonia;
using Gitea.Client;
using ReactiveUI;
using Serilog;
using SPTInstaller.Models;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace SPTInstaller.ViewModels;

public class MainWindowViewModel : ReactiveObject, IActivatableViewModel, IScreen
{
    public RoutingState Router { get; } = new();
    public ViewModelActivator Activator { get; } = new();
    public InstallerUpdateInfo UpdateInfo { get; } = new();

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

        var uiCulture= CultureInfo.InstalledUICulture;

        Log.Information("System Language: {iso} - {name}", uiCulture.TwoLetterISOLanguageName, uiCulture.DisplayName);

        Task.Run(async () =>
        {
            await UpdateInfo.CheckForUpdates(version);
        });

        Router.Navigate.Execute(new PreChecksViewModel(this, DismissUpdateCommand));
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

    public void DismissUpdateCommand()
    {
        UpdateInfo.UpdateAvailable = false;
    }

    public async Task UpdateInstallerCommand()
    {
        Router.Navigate.Execute(new MessageViewModel(this, Result.FromSuccess("Please wait while the update is installed"), false));
        await UpdateInfo.UpdateInstaller();
    }
}