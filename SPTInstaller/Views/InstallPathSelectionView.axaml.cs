using Avalonia.Controls;
using Avalonia.ReactiveUI;
using SPTInstaller.ViewModels;

namespace SPTInstaller.Views;

public partial class InstallPathSelectionView : ReactiveUserControl<InstallPathSelectionViewModel>
{
    public InstallPathSelectionView()
    {
        InitializeComponent();
    }
    
    private void TextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        ViewModel?.ValidatePath();
    }
}