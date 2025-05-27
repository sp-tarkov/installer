using System.Threading.Tasks;
using ReactiveUI;

namespace SPTInstaller.ViewModels;

public class OverviewViewModel : ViewModelBase
{
    private string _providedPath;
    public OverviewViewModel(IScreen Host, string providedPath) : base(Host)
    {
        _providedPath = providedPath;
        
        if (!string.IsNullOrEmpty(_providedPath))
        {
            Task.Run(NextCommand);
        }
    }
    
    public void NextCommand()
    {
        NavigateTo(new InstallPathSelectionViewModel(HostScreen, _providedPath));
    }
}