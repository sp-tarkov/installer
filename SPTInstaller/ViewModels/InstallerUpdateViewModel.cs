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
    
    private string _installPath;
    public InstallerUpdateViewModel(IScreen Host, string installPath) : base(Host)
    {
        _installPath = installPath;
        
        _data = ServiceHelper.Get<InternalData>() ?? throw new Exception("Failed to get internal data");
        
        Task.Run(async () =>
        {
            await UpdateInfo.CheckForUpdates(Assembly.GetExecutingAssembly().GetName().Version);
            
            if (!UpdateInfo.UpdateAvailable)
            {
                NavigateTo(new OverviewViewModel(HostScreen, _installPath));
            }
        });
    }
    
    public void NotNowCommand()
    {
        NavigateTo(new OverviewViewModel(HostScreen, _installPath));
    }
    
    public async Task UpdateInstallCommand()
    {
        await UpdateInfo.UpdateInstaller();
    }
}