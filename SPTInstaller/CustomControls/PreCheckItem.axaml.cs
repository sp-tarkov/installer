using Avalonia;
using Avalonia.Controls;

namespace SPTInstaller.CustomControls;

public partial class PreCheckItem : UserControl
{
    public PreCheckItem()
    {
        InitializeComponent();
    }

    public string PreCheckName
    {
        get => GetValue(PreCheckNameProperty);
        set => SetValue(PreCheckNameProperty, value);
    }

    public static readonly StyledProperty<string> PreCheckNameProperty =
        AvaloniaProperty.Register<PreCheckItem, string>(nameof(PreCheckName));

    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<PreCheckItem, bool>(nameof(IsRequired));

    public StatusSpinner.SpinnerState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly StyledProperty<StatusSpinner.SpinnerState> StateProperty =
        AvaloniaProperty.Register<PreCheckItem, StatusSpinner.SpinnerState>(nameof(State));
}