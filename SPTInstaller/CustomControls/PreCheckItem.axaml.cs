using System.Windows.Input;
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


    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<PreCheckItem, bool>(nameof(IsSelected));

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly StyledProperty<ICommand> SelectCommandProperty =
        AvaloniaProperty.Register<PreCheckItem, ICommand>(nameof(SelectCommand));

    public ICommand SelectCommand
    {
        get => GetValue(SelectCommandProperty);
        set => SetValue(SelectCommandProperty, value);
    }
}