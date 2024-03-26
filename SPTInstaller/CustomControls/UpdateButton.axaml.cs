using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SPTInstaller.CustomControls;

public partial class UpdateButton : UserControl
{
    public UpdateButton()
    {
        InitializeComponent();
    }
    
    
    
    // InfoText="{Binding UpdateInfo.UpdateInfoText}"
    // ShowUpdateCard="{Binding UpdateInfo.ShowCard}"
    // NotNowCommand="{Binding DismissUpdateCommand}"
    // UpdateInstallerCommand="{Binding UpdateInstallerCommand}"
    // Updating="{Binding UpdateInfo.Updating}"
    // DownloadProgress="{Binding UpdateInfo.DownloadProgress}"
    // IndeterminateProgress="{Binding UpdateInfo.CheckingForUpdates}"
    // UpdateAvailable="{Binding UpdateInfo.UpdateAvailable}"
}