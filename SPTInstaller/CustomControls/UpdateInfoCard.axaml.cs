using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

namespace SPTInstaller.CustomControls;
public partial class UpdateInfoCard : UserControl
{
    public UpdateInfoCard()
    {
        InitializeComponent();
    }

    public bool ShowUpdateCard
    {
        get => GetValue(ShowUpdateCardProperty);
        set => SetValue(ShowUpdateCardProperty, value);
    }
    public static readonly StyledProperty<bool> ShowUpdateCardProperty =
        AvaloniaProperty.Register<UpdateInfoCard, bool>(nameof(ShowUpdateCard));

    public bool Updating
    {
        get => GetValue(UpdatingProperty);
        set => SetValue(UpdatingProperty, value);
    }
    public static readonly StyledProperty<bool> UpdatingProperty =
        AvaloniaProperty.Register<UpdateInfoCard, bool>(nameof(Updating));

    public bool UpdateAvailable
    {
        get => GetValue(UpdateAvailableProperty);
        set => SetValue(UpdateAvailableProperty, value);
    }
    public static readonly StyledProperty<bool> UpdateAvailableProperty =
        AvaloniaProperty.Register<UpdateInfoCard, bool>(nameof(UpdateAvailable));

    public bool IndeterminateProgress
    {
        get => GetValue(IndeterminateProgressProperty);
        set => SetValue(IndeterminateProgressProperty, value);
    }
    public static readonly StyledProperty<bool> IndeterminateProgressProperty =
        AvaloniaProperty.Register<UpdateInfoCard, bool>(nameof(IndeterminateProgress));

    public string InfoText
    {
        get => GetValue(InfoTextProperty);
        set => SetValue(InfoTextProperty, value);
    }
    public static readonly StyledProperty<string> InfoTextProperty =
        AvaloniaProperty.Register<UpdateInfoCard, string>(nameof(InfoText));

    public int DownloadProgress
    {
        get => GetValue(DownloadProgressProperty);
        set => SetValue(DownloadProgressProperty, value);
    }
    public static readonly StyledProperty<int> DownloadProgressProperty =
        AvaloniaProperty.Register<UpdateInfoCard, int>(nameof(DownloadProgress));

    public ICommand NotNowCommand
    {
        get => GetValue(NotNowCommandProperty);
        set => SetValue(NotNowCommandProperty, value);
    }
    public static readonly StyledProperty<ICommand> NotNowCommandProperty =
        AvaloniaProperty.Register<UpdateInfoCard, ICommand>(nameof(NotNowCommand));

    public ICommand UpdateInstallerCommand
    {
        get => GetValue(UpdateInstallerCommandProperty);
        set => SetValue(UpdateInstallerCommandProperty, value);
    }
    public static readonly StyledProperty<ICommand> UpdateInstallerCommandProperty =
        AvaloniaProperty.Register<UpdateInfoCard, ICommand>(nameof(UpdateInstallerCommand));
}
