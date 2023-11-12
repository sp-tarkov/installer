using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Serilog;
using SPTInstaller.ViewModels;
using SPTInstaller.Views;
using System.Reactive;

namespace SPTInstaller;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var logPath = Path.Join(Environment.CurrentDirectory, "spt-aki-installer_.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo
            .File(path: logPath,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug,
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        RxApp.DefaultExceptionHandler = Observer.Create<Exception>((exception) =>
        {
            Log.Error(exception, "An application exception occurred");
        });

    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.Args != null && desktop.Args.Any(x => x.ToLower() == "debug"))
            {
                System.Diagnostics.Trace.Listeners.Add(new SerilogTraceListener.SerilogTraceListener());
                Log.Debug("TraceListener is registered");
            }
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}