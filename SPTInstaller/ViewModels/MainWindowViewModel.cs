using Avalonia;
using ReactiveUI;
using Serilog;
using System;

namespace SPTInstaller.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IActivatableViewModel, IScreen
    {
        public RoutingState Router { get; } = new RoutingState();
        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public MainWindowViewModel()
        {
            Log.Information("========= LAUNCHER STARTED =========");
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
}