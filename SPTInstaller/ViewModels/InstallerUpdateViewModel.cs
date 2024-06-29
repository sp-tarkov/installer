using ReactiveUI;

namespace SPTInstaller.ViewModels;

public class InstallerUpdateViewModel : ViewModelBase
{
    private bool _debugging;
    public InstallerUpdateViewModel(IScreen Host, bool debugging) : base(Host)
    {
        _debugging = debugging;
    }
}