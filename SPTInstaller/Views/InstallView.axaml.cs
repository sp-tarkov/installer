using Avalonia.Controls;
using Avalonia.ReactiveUI;
using SPTInstaller.ViewModels;

namespace SPTInstaller.Views
{
    public partial class InstallView : ReactiveUserControl<InstallViewModel>
    {
        public InstallView()
        {
            InitializeComponent();
        }
    }
}
