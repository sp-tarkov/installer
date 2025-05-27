using Avalonia.ReactiveUI;
using SPTInstaller.ViewModels;

namespace SPTInstaller.Views;

public partial class PreChecksView : ReactiveUserControl<PreChecksViewModel>
{
    public PreChecksView()
    {
        InitializeComponent();
    }
}