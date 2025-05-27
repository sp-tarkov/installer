using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace SPTInstaller.CustomControls;

public partial class StatusSpinner : ReactiveUserControl<UserControl>
{
    public enum SpinnerState
    {
        Pending = -1,
        Running = 0,
        OK = 1,
        Warning = 2,
        Error = 3,
    }
    
    public StatusSpinner()
    {
        InitializeComponent();
    }
    
    public SpinnerState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }
    
    public static readonly StyledProperty<SpinnerState> StateProperty =
        AvaloniaProperty.Register<StatusSpinner, SpinnerState>(nameof(State));
}