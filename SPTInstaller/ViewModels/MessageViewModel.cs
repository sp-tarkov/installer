using Avalonia;
using ReactiveUI;
using System.Windows.Input;

namespace SPTInstaller.ViewModels
{
    public class MessageViewModel : ViewModelBase
    {
        private string _Message;
        public string Message
        {
            get => _Message;
            set => this.RaiseAndSetIfChanged(ref _Message, value);
        }

        public ICommand CloseCommand { get; set; } = ReactiveCommand.Create(() =>
        {
            if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.MainWindow.Close();
            }
        });

        public MessageViewModel(IScreen Host, string message) : base(Host)
        {
            Message = message;
        }
    }
}
