using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using ReactiveUI;
using System.Windows.Input;

namespace SPTInstaller.CustomControls
{
    public partial class PreCheckItem : UserControl
    {
        public PreCheckItem()
        {
            InitializeComponent();
        }

        public string PreCheckName
        {
            get => GetValue(PreCheckNameProperty);
            set => SetValue(PreCheckNameProperty, value);
        }

        public static readonly StyledProperty<string> PreCheckNameProperty =
            AvaloniaProperty.Register<PreCheckItem, string>(nameof(PreCheckName));

        public bool IsRunning
        {
            get => GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);    
        }

        public static readonly StyledProperty<bool> IsRunningProperty =
            AvaloniaProperty.Register<PreCheckItem, bool>(nameof(IsRunning));

        public bool IsPending
        {
            get => GetValue(IsPendingProperty);
            set => SetValue(IsPendingProperty, value);
        }

        public static readonly StyledProperty<bool> IsPendingProperty =
            AvaloniaProperty.Register<PreCheckItem, bool>(nameof(IsPending));

        public bool Passed
        {
            get => GetValue(PassedProperty);
            set => SetValue(PassedProperty, value);
        }

        public static readonly StyledProperty<bool> PassedProperty =
            AvaloniaProperty.Register<PreCheckItem, bool>(nameof(Passed));

        public bool IsRequired
        {
            get => GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        public static readonly StyledProperty<bool> IsRequiredProperty =
            AvaloniaProperty.Register<PreCheckItem, bool>(nameof(IsRequired));
    }
}
