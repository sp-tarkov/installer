using Avalonia;

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
}
