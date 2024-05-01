using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tmds.DBus.Protocol;

namespace SPTInstaller.CustomControls.Dialogs;

public partial class ChangeLogDialog : UserControl
{
    public string Message { get; set; }
    public string Version { get; set; }
    public ChangeLogDialog(string newVersion, string message)
    {
        InitializeComponent();
        Message = message;
        Version = newVersion;
    }
    
    // public static readonly StyledProperty<string> MessageProperty =
    //     AvaloniaProperty.Register<ChangeLogDialog, string>("Message");
    //
    // public string Message
    // {
    //     get => GetValue(MessageProperty);
    //     set => SetValue(MessageProperty, value);
    // }
}