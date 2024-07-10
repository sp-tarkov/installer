using Avalonia.ReactiveUI;
using SPTInstaller.ViewModels;

namespace SPTInstaller.Views;

public partial class OverviewView : ReactiveUserControl<OverviewViewModel>
{
    public OverviewView()
    {
        InitializeComponent();
    }
}