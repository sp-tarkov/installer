using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Serilog;
using SPTInstaller.ViewModels;
using SPTInstaller.Views;
using System.Reactive;
using System.Text;
using DynamicData;

namespace SPTInstaller;

public partial class App : Application
{
    public static string LogPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "spt-installer", "spt-installer.log");
    
    public static void ReLaunch(bool debug, string installPath = "")
    {
        var installerPath = Path.Join(Environment.CurrentDirectory, "SPTInstaller.exe");
        
        var args = new StringBuilder()
            .Append(debug ? "debug " : "")
            .Append(!string.IsNullOrEmpty(installPath) ? $"installPath=\"{installPath}\"" : "")
            .ToString();
        
        Process.Start(new ProcessStartInfo()
        {
            FileName = installerPath,
            Arguments = args
        });
        
        Environment.Exit(0);
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo
            .File(path: LogPath,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
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
            var debug = false;
            var providedPath = "";
            
            if (desktop.Args != null)
            {
                debug = desktop.Args.Any(x => x.ToLower() == "debug");
                var installPath = desktop.Args.FirstOrDefault(x => x.StartsWith("installPath=", StringComparison.CurrentCultureIgnoreCase));
                
                providedPath = installPath != null && installPath.Contains('=') ? installPath?.Split('=')[1] ?? "" : "";
            }
            
            if (debug)
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo
                    .File(path: LogPath,
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
                    .CreateLogger();
                
                System.Diagnostics.Trace.Listeners.Add(new SerilogTraceListener.SerilogTraceListener());
                
                Log.Debug("TraceListener is registered");
            }
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(providedPath, debug),
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}