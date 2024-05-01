using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData.Binding;
using SPTInstaller.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SPTInstaller.CustomControls;

public partial class ProgressableTaskList : UserControl
{
    public ProgressableTaskList()
    {
        InitializeComponent();
        
        this.AttachedToVisualTree += ProgressableTaskList_AttachedToVisualTree;
    }
    
    private int _taskProgress;
    
    public int TaskProgress
    {
        get => _taskProgress;
        set => SetAndRaise(ProgressableTaskList.TaskProgressProperty, ref _taskProgress, value);
    }
    
    public static readonly DirectProperty<ProgressableTaskList, int> TaskProgressProperty =
        AvaloniaProperty.RegisterDirect<ProgressableTaskList, int>(nameof(TaskProgress), o => o.TaskProgress);
    
    public ObservableCollection<InstallerTaskBase> Tasks
    {
        get => GetValue(TasksProperty);
        set => SetValue(TasksProperty, value);
    }
    
    public static readonly StyledProperty<ObservableCollection<InstallerTaskBase>> TasksProperty =
        AvaloniaProperty.Register<ProgressableTaskList, ObservableCollection<InstallerTaskBase>>(nameof(Tasks));
    
    public IBrush PendingColor
    {
        get => GetValue(PendingColorProperty);
        set => SetValue(PendingColorProperty, value);
    }
    
    public static readonly StyledProperty<IBrush> PendingColorProperty =
        AvaloniaProperty.Register<ProgressableTaskList, IBrush>(nameof(PendingColor));
    
    public IBrush RunningColor
    {
        get => GetValue(RunningColorProperty);
        set => SetValue(RunningColorProperty, value);
    }
    
    public static readonly StyledProperty<IBrush> RunningColorProperty =
        AvaloniaProperty.Register<ProgressableTaskList, IBrush>(nameof(PendingColor));
    
    public IBrush CompletedColor
    {
        get => GetValue(CompletedColorProperty);
        set => SetValue(CompletedColorProperty, value);
    }
    
    public static readonly StyledProperty<IBrush> CompletedColorProperty =
        AvaloniaProperty.Register<ProgressableTaskList, IBrush>(nameof(PendingColor));
    
    private void UpdateTaskProgress()
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var completedTasks = Tasks.Where(x => x.IsCompleted == true).Count();
            
            var progress = (int)Math.Floor((double)completedTasks / (Tasks.Count - 1) * 100);
            
            for (; TaskProgress < progress;)
            {
                TaskProgress += 1;
                await Task.Delay(1);
            }
        });
    }
    
    private void ProgressableTaskList_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (Tasks == null) return;
        
        foreach (var task in Tasks)
        {
            task.WhenPropertyChanged(x => x.IsCompleted)
                .Subscribe(x => UpdateTaskProgress());
        }
    }
}