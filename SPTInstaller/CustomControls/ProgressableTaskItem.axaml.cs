using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SPTInstaller.CustomControls;

public partial class ProgressableTaskItem : UserControl
{
    public ProgressableTaskItem()
    {
        InitializeComponent();
    }
    
    public string TaskId
    {
        get => GetValue(TaskIdProperty);
        set => SetValue(TaskIdProperty, value);
    }
    
    public static readonly StyledProperty<string> TaskIdProperty =
        AvaloniaProperty.Register<ProgressableTaskItem, string>(nameof(TaskId));
    
    public string TaskName
    {
        get => GetValue(TaskNameProperty);
        set => SetValue(TaskNameProperty, value);
    }
    
    public static readonly StyledProperty<string> TaskNameProperty =
        AvaloniaProperty.Register<ProgressableTaskItem, string>(nameof(TaskName));
    
    public bool IsCompleted
    {
        get => GetValue(IsCompletedProperty);
        set => SetValue(IsCompletedProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsCompletedProperty =
        AvaloniaProperty.Register<ProgressableTaskItem, bool>(nameof(IsCompleted));
    
    public bool IsRunning
    {
        get => GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsRunningProperty =
        AvaloniaProperty.Register<ProgressableTaskItem, bool>(nameof(IsRunning));
    
    public IBrush PendingColor
    {
        get => GetValue(PendingColorProperty);
        set => SetValue(PendingColorProperty, value);
    }
    
    public static readonly StyledProperty<IBrush> PendingColorProperty =
        AvaloniaProperty.Register<ProgressableTaskItem, IBrush>(nameof(PendingColor));
    
    public IBrush RunningColor
    {
        get => GetValue(RunningColorProperty);
        set => SetValue(RunningColorProperty, value);
    }
    
    public static readonly StyledProperty<IBrush> RunningColorProperty =
        AvaloniaProperty.Register<ProgressableTaskItem, IBrush>(nameof(PendingColor));
    
    public IBrush CompletedColor
    {
        get => GetValue(CompletedColorProperty);
        set => SetValue(CompletedColorProperty, value);
    }
    
    public static readonly StyledProperty<IBrush> CompletedColorProperty =
        AvaloniaProperty.Register<ProgressableTaskItem, IBrush>(nameof(PendingColor));
}