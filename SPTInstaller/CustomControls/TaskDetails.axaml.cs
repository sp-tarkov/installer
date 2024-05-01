using Avalonia;
using Avalonia.Controls;

namespace SPTInstaller.CustomControls;

public partial class TaskDetails : UserControl
{
    public TaskDetails()
    {
        InitializeComponent();
    }
    
    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
    
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<TaskDetails, string>(nameof(Message));
    
    public string Details
    {
        get => GetValue(DetailsProperty);
        set => SetValue(DetailsProperty, value);
    }
    
    public static readonly StyledProperty<string> DetailsProperty =
        AvaloniaProperty.Register<TaskDetails, string>(nameof(Details));
    
    public int Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }
    
    public static readonly StyledProperty<int> ProgressProperty =
        AvaloniaProperty.Register<TaskDetails, int>(nameof(Progress));
    
    public bool ShowProgress
    {
        get => GetValue(ShowProgressProperty);
        set => SetValue(ShowProgressProperty, value);
    }
    
    public static readonly StyledProperty<bool> ShowProgressProperty =
        AvaloniaProperty.Register<TaskDetails, bool>(nameof(ShowProgress));
    
    public bool IndeterminateProgress
    {
        get => GetValue(IndeterminateProgressProperty);
        set => SetValue(IndeterminateProgressProperty, value);
    }
    
    public static readonly StyledProperty<bool> IndeterminateProgressProperty =
        AvaloniaProperty.Register<TaskDetails, bool>(nameof(IndeterminateProgress));
}