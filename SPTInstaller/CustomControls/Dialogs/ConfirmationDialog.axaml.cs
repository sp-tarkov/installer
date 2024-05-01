using Avalonia;
using Avalonia.Controls;

namespace SPTInstaller.CustomControls.Dialogs;

public partial class ConfirmationDialog : UserControl
{
    public ConfirmationDialog(string message)
    {
        InitializeComponent();
        
        Message = message;
    }
    
    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
    
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<ConfirmationDialog, string>(nameof(Message));
}