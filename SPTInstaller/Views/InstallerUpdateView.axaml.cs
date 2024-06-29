using Avalonia.ReactiveUI;
using SPTInstaller.ViewModels;

namespace SPTInstaller.Views;

public partial class InstallerUpdateView : ReactiveUserControl<InstallerUpdateViewModel>
{
    public InstallerUpdateView()
    {
        InitializeComponent();
    }
}