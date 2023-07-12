using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using SPTInstaller.Aki.Helper;
using SPTInstaller.Controllers;
using SPTInstaller.Helpers;
using SPTInstaller.Models;

namespace SPTInstaller.ViewModels;

public class PreChecksViewModel : ViewModelBase
{
    private string _installPath;

    public ObservableCollection<PreCheckBase> PreChecks { get; set; } = new(ServiceHelper.GetAll<PreCheckBase>());
    public ICommand StartInstallCommand { get; set; }
    public bool PreCheckSucceeded { get; set; }
    public string InstallPath
    {
        get => _installPath;
        set => this.RaiseAndSetIfChanged(ref _installPath, value);
    }

    public PreChecksViewModel(IScreen host) : base(host)
    {
        var data = ServiceHelper.Get<InternalData?>();
        var installer = ServiceHelper.Get<InstallController?>();

        if (data == null || installer == null)
        {
            NavigateTo(new MessageViewModel(HostScreen, Result.FromError("Failed to get required service for prechecks")));
            return;
        }

        data.OriginalGamePath = PreCheckHelper.DetectOriginalGamePath();
        data.TargetInstallPath = Environment.CurrentDirectory;
        InstallPath = data.TargetInstallPath;

        StartInstallCommand = ReactiveCommand.Create(() => NavigateTo(new InstallViewModel(HostScreen)));

        Task.Run(async () =>
        {
            var result = await installer.RunPreChecks();
            PreCheckSucceeded = result.Succeeded;
        });
    }
}