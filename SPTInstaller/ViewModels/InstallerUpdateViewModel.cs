using System.Reflection;
using System.Threading.Tasks;
using ReactiveUI;
using SPTInstaller.Helpers;
using SPTInstaller.Models;

namespace SPTInstaller.ViewModels;

public class InstallerUpdateViewModel : ViewModelBase
{
    
    public InstallerUpdateInfo UpdateInfo { get; set; } = new();
    private InternalData _data;
    
    private bool _debugging;
    public InstallerUpdateViewModel(IScreen Host, bool debugging) : base(Host)
    {
        _debugging = debugging;
        _data = ServiceHelper.Get<InternalData>() ?? throw new Exception("Failed to get internal data");
        
        Task.Run(async () =>
        {
            await UpdateInfo.CheckForUpdates(Assembly.GetExecutingAssembly().GetName().Version);
            
            if (!UpdateInfo.UpdateAvailable)
            {
                NavigateTo(new InstallPathSelectionViewModel(HostScreen, _debugging));
            }
        });
    }
    
    public void NotNowCommand()
    {
        NavigateTo(new InstallPathSelectionViewModel(HostScreen, _debugging));
    }
    
    public async Task UpdateInstallCommand()
    {
        await UpdateInfo.UpdateInstaller();
    }
}