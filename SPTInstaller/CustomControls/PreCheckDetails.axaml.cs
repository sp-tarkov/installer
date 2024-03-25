using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace SPTInstaller.CustomControls;

public partial class PreCheckDetails : UserControl
{
    public PreCheckDetails()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> PreCheckNameProperty =
        AvaloniaProperty.Register<PreCheckDetails, string>(nameof(PreCheckName));

    public string PreCheckName
    {
        get => GetValue(PreCheckNameProperty);
        set => SetValue(PreCheckNameProperty, value);
    }

    public static readonly StyledProperty<string> DetailsProperty =
        AvaloniaProperty.Register<PreCheckDetails, string>(nameof(Details));

    public string Details
    {
        get => GetValue(DetailsProperty);
        set => SetValue(DetailsProperty, value);
    }

    public static readonly StyledProperty<string> ActionButtonTextProperty =
        AvaloniaProperty.Register<PreCheckDetails, string>(nameof(ActionButtonText));

    public string ActionButtonText
    {
        get => GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    public static readonly StyledProperty<ICommand> ActionCommandProperty =
        AvaloniaProperty.Register<PreCheckDetails, ICommand>(nameof(ActionCommand));

    public ICommand ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public static readonly StyledProperty<bool> ShowActionProperty =
        AvaloniaProperty.Register<PreCheckDetails, bool>(nameof(ShowAction));

    public bool ShowAction
    {
        get => GetValue(ShowActionProperty);
        set => SetValue(ShowActionProperty, value);
    }

    public static readonly StyledProperty<string> BarColorProperty =
        AvaloniaProperty.Register<PreCheckDetails, string>(nameof(BarColor));

    public string BarColor
    {
        get => GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }
}