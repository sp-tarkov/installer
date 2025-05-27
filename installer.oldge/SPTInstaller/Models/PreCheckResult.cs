using ReactiveUI;
using SPTInstaller.Interfaces;
using System.Windows.Input;

namespace SPTInstaller.Models;

public class PreCheckResult : IResult
{
    public bool Succeeded { get; private set; }
    
    public string Message { get; private set; }
    
    public bool ActionButtonIsVisible { get; private set; }
    
    public string ActionButtonText { get; private set; }
    
    public ICommand ButtonPressedCommand { get; private set; }
    
    protected PreCheckResult(string message, bool succeeded, string actionButtonText, Action? buttonPressedAction)
    {
        Message = message;
        Succeeded = succeeded;
        
        ActionButtonText = actionButtonText;
        
        ActionButtonIsVisible = buttonPressedAction != null && !string.IsNullOrWhiteSpace(actionButtonText);
        
        buttonPressedAction ??= () => { };
        
        ButtonPressedCommand = ReactiveCommand.Create(buttonPressedAction);
    }
    
    public static PreCheckResult FromSuccess(string message = "") => new PreCheckResult(message, true, "", null);
    
    public static PreCheckResult FromError(string message, string actionButtonText = "",
        Action? actionButtonPressedAction = null) =>
        new PreCheckResult(message, false, actionButtonText, actionButtonPressedAction);
    
    public static PreCheckResult
        FromException(Exception ex, string actionButtonText = "", Action? actionButtonPressedAction = null) =>
        new PreCheckResult(
            $"An exception was thrown during this precheck\n\nException:\n{ex.Message}\n\nStacktrace:\n{ex.StackTrace}",
            false, actionButtonText, actionButtonPressedAction);
}