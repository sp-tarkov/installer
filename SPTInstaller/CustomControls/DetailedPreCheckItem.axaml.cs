using Avalonia;
using System.Windows.Input;

namespace SPTInstaller.CustomControls;
public partial class DetailedPreCheckItem : PreCheckItem
{
    public DetailedPreCheckItem()
    {
        InitializeComponent();
    }

    public string PreCheckDetails
    {
        get => GetValue(PreCheckDetailsProperty);
        set => SetValue(PreCheckDetailsProperty, value);
    }

    public static readonly StyledProperty<string> PreCheckDetailsProperty =
        AvaloniaProperty.Register<DetailedPreCheckItem, string>(nameof(PreCheckDetails));

    public bool ActionButtonIsVisible
    {
        get => GetValue(ActionButtonIsVisibleProperty);
        set => SetValue(ActionButtonIsVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> ActionButtonIsVisibleProperty =
        AvaloniaProperty.Register<DetailedPreCheckItem, bool>(nameof(ActionButtonIsVisible));

    public string ActionButtonText
    {
        get => GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    public static readonly StyledProperty<string> ActionButtonTextProperty =
        AvaloniaProperty.Register<DetailedPreCheckItem, string>(nameof(ActionButtonText));

    public ICommand ActionButtonCommand
    {
        get => GetValue(ActionButtonCommandProperty);
        set => SetValue(ActionButtonCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> ActionButtonCommandProperty =
        AvaloniaProperty.Register<DetailedPreCheckItem, ICommand>(nameof(ActionButtonCommand));

}
