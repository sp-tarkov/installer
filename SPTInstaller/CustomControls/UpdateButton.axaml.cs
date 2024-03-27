using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace SPTInstaller.CustomControls;

public partial class UpdateButton : UserControl
{
    public UpdateButton()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> InfoTextProperty = AvaloniaProperty.Register<UpdateButton, string>(
        "InfoText");

    public string InfoText
    {
        get => GetValue(InfoTextProperty);
        set => SetValue(InfoTextProperty, value);
    }

    public static readonly StyledProperty<bool> CheckingForUpdateProperty = AvaloniaProperty.Register<UpdateButton, bool>(
        "CheckingForUpdate");

    public bool CheckingForUpdate
    {
        get => GetValue(CheckingForUpdateProperty);
        set => SetValue(CheckingForUpdateProperty, value);
    }

    public static readonly StyledProperty<ICommand> DismissCommandProperty = AvaloniaProperty.Register<UpdateButton, ICommand>(
        "DismissCommand");

    public ICommand DismissCommand
    {
        get => GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> UpdateCommandProperty = AvaloniaProperty.Register<UpdateButton, ICommand>(
        "UpdateCommand");

    public ICommand UpdateCommand
    {
        get => GetValue(UpdateCommandProperty);
        set => SetValue(UpdateCommandProperty, value);
    }

    public static readonly StyledProperty<bool> UpdatingProperty = AvaloniaProperty.Register<UpdateButton, bool>(
        "Updating");

    public bool Updating
    {
        get => GetValue(UpdatingProperty);
        set => SetValue(UpdatingProperty, value);
    }

    public static readonly StyledProperty<int> DownloadProgressProperty = AvaloniaProperty.Register<UpdateButton, int>(
        "DownloadProgress");

    public int DownloadProgress
    {
        get => GetValue(DownloadProgressProperty);
        set => SetValue(DownloadProgressProperty, value);
    }

    public static readonly StyledProperty<bool> IsIndeterminateProperty = AvaloniaProperty.Register<UpdateButton, bool>(
        "IsIndeterminate");

    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public static readonly StyledProperty<bool> UpdateAvailableProperty = AvaloniaProperty.Register<UpdateButton, bool>(
        "UpdateAvailable");

    public bool UpdateAvailable
    {
        get => GetValue(UpdateAvailableProperty);
        set => SetValue(UpdateAvailableProperty, value);
    }
}