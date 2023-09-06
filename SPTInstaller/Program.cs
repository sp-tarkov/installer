using Avalonia;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Serilog;
using Splat;
using SPTInstaller.Controllers;
using SPTInstaller.Helpers;
using SPTInstaller.Installer_Tasks;
using SPTInstaller.Installer_Tasks.PreChecks;
using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Linq;
using System.Reflection;

namespace SPTInstaller;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

        // Register all the things
        // Regestering as base classes so ReactiveUI works correctly. Doesn't seem to like the interfaces :(
        ServiceHelper.Register<InternalData>();

#if !TEST
        ServiceHelper.Register<PreCheckBase, NetFramework472PreCheck>();
        ServiceHelper.Register<PreCheckBase, NetCore6PreCheck>();

        ServiceHelper.Register<PreCheckBase, FreeSpacePreCheck>();
        var logPath = Path.Join(Environment.CurrentDirectory, "spt-aki-installer_.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo
            .File(path: logPath,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug, 
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        ServiceHelper.Register<InstallerTaskBase, InitializationTask>();
        ServiceHelper.Register<InstallerTaskBase, ReleaseCheckTask>();
        ServiceHelper.Register<InstallerTaskBase, DownloadTask>();
        ServiceHelper.Register<InstallerTaskBase, CopyClientTask>();
        ServiceHelper.Register<InstallerTaskBase, SetupClientTask>();
#else
        for (int i = 0; i < 5; i++)
        {
            Locator.CurrentMutable.RegisterConstant<InstallerTaskBase>(TestTask.FromRandomName());
        }

        Locator.CurrentMutable.RegisterConstant<PreCheckBase>(TestPreCheck.FromRandomName(StatusSpinner.SpinnerState.OK));
        Locator.CurrentMutable.RegisterConstant<PreCheckBase>(TestPreCheck.FromRandomName(StatusSpinner.SpinnerState.Warning));
        Locator.CurrentMutable.RegisterConstant<PreCheckBase>(TestPreCheck.FromRandomName(StatusSpinner.SpinnerState.Error));
#endif

        // need the interfaces for the controller and splat won't resolve them since we need to base classes in avalonia (what a mess), so doing it manually here
        var tasks = Locator.Current.GetServices<InstallerTaskBase>().ToArray() as IProgressableTask[];
        var preChecks = Locator.Current.GetServices<PreCheckBase>().ToArray() as IPreCheck[];

        var installer = new InstallController(tasks, preChecks);

        // manually register install controller
        Locator.CurrentMutable.RegisterConstant(installer);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }
}