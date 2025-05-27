using Avalonia.ReactiveUI;
using SPTInstaller.ViewModels;

namespace SPTInstaller.Views;

public partial class MessageView : ReactiveUserControl<MessageViewModel>
{
    public MessageView()
    {
        InitializeComponent();
    }
}