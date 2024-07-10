using System.Threading.Tasks;
using ReactiveUI;

namespace SPTInstaller.ViewModels;

public class OverviewViewModel : ViewModelBase
{
    private string _providedPath;
    private bool _debugging;
    public OverviewViewModel(IScreen Host, string providedPath, bool debugging) : base(Host)
    {
        _providedPath = providedPath;
        _debugging = debugging;
        
        if (!string.IsNullOrEmpty(_providedPath))
        {
            Task.Run(NextCommand);
        }
    }
    
    public void NextCommand()
    {
        NavigateTo(new InstallPathSelectionViewModel(HostScreen, _providedPath, _debugging));
    }
}