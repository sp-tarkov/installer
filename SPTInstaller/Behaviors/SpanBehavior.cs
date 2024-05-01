using Avalonia;
using Avalonia.Interactivity;

namespace SPTInstaller.Behaviors;

public class SpanBehavior : AvaloniaObject
{
    public static readonly AttachedProperty<bool> SpanProperty =
        AvaloniaProperty.RegisterAttached<SpanBehavior, Interactive, bool>("Span");
    
    public static void SetSpan(AvaloniaObject element, bool value)
    {
        element.SetValue(SpanProperty, value);
    }
    
    public static bool GetSpan(AvaloniaObject element)
    {
        return element.GetValue(SpanProperty);
    }
}