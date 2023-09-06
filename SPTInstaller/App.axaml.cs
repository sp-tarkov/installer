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

        RxApp.DefaultExceptionHandler = Observer.Create<Exception>((exception) =>
        {
            Log.Error(exception, "An application exception occurred");
        });

    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}