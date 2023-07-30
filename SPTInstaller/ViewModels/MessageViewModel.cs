using Avalonia;
using ReactiveUI;
using Serilog;
using SPTInstaller.Interfaces;
using System.Windows.Input;

namespace SPTInstaller.ViewModels;

public class MessageViewModel : ViewModelBase
{
    private bool _HasErrors;
    public bool HasErrors
    {
        get => _HasErrors;
        set => this.RaiseAndSetIfChanged(ref _HasErrors, value);
    }

    private string _Message;
    public string Message
    {
        get => _Message;
        set => this.RaiseAndSetIfChanged(ref _Message, value);
    }

    private bool _showCloseButton;
    public bool ShowCloseButton
    {
        get => _showCloseButton;
        set => this.RaiseAndSetIfChanged(ref _showCloseButton, value);
    }

    public ICommand CloseCommand { get; set; } = ReactiveCommand.Create(() =>
    {
        if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.MainWindow.Close();
        }
    });

    public MessageViewModel(IScreen Host, IResult result, bool showCloseButton = true) : base(Host)
    {
        ShowCloseButton = showCloseButton;
        Message = result.Message;

        if(result.Succeeded)
        {
            Log.Information(Message);
            return;
        }

        HasErrors = true;
        Log.Error(Message);
    }
}