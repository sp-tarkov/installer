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
    private readonly string _logPath = Path.Join(Environment.CurrentDirectory, "spt-aki-installer_.log");
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo
            .File(path: _logPath,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
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
            var debug = desktop.Args != null && desktop.Args.Any(x => x.ToLower() == "debug");
            if (debug)
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo
                    .File(path: _logPath,
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug,
                        rollingInterval: RollingInterval.Day)
                    .CreateLogger();
                
                System.Diagnostics.Trace.Listeners.Add(new SerilogTraceListener.SerilogTraceListener());
                
                Log.Debug("TraceListener is registered");
            }
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(debug),
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}