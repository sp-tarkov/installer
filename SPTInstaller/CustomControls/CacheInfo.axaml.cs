using Avalonia;
using Avalonia.Controls;
using DialogHostAvalonia;
using SPTInstaller.CustomControls.Dialogs;
using System.Threading.Tasks;

namespace SPTInstaller.CustomControls;

public partial class CacheInfo : UserControl
{
    public CacheInfo()
    {
        InitializeComponent();
    }
    
    public async Task ShowCacheDialogCommand() => await DialogHost.Show(new WhyCacheThoughDialog());
    
    public string InfoText
    {
        get => GetValue(InfoTextProperty);
        set => SetValue(InfoTextProperty, value);
    }
    
    public static readonly StyledProperty<string> InfoTextProperty =
        AvaloniaProperty.Register<CacheInfo, string>(nameof(InfoText));
    
    public StatusSpinner.SpinnerState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }
    
    public static readonly StyledProperty<StatusSpinner.SpinnerState> StateProperty =
        AvaloniaProperty.Register<CacheInfo, StatusSpinner.SpinnerState>(nameof(State));
}