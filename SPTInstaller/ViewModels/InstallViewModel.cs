using ReactiveUI;
using SPTInstaller.Controllers;
using SPTInstaller.Helpers;
using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SPTInstaller.ViewModels;

public class InstallViewModel : ViewModelBase
{
    private IProgressableTask _currentTask;
    public IProgressableTask CurrentTask
    {
        get => _currentTask;
        set => this.RaiseAndSetIfChanged(ref _currentTask, value);
    }

    public ObservableCollection<InstallerTaskBase> MyTasks { get; set; } = new(ServiceHelper.GetAll<InstallerTaskBase>());

    public InstallViewModel(IScreen host) : base(host)
    {
        var installer = ServiceHelper.Get<InstallController>();

        installer.TaskChanged += (sender, task) => CurrentTask = task;

        Task.Run(async () => 
        {
            var result = await installer.RunTasks();

            NavigateTo(new MessageViewModel(HostScreen, result));
        });
    }
}