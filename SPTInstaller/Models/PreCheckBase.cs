using ReactiveUI;
using SPTInstaller.Interfaces;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SPTInstaller.Models;

public abstract class PreCheckBase : ReactiveObject, IPreCheck
{
    private string _id;
    public string Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private string _name;
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private bool _required;
    public bool IsRequired
    {
        get => _required;
        set => this.RaiseAndSetIfChanged(ref _required, value);
    }

    private bool _passed;
    public bool Passed
    {
        get => _passed;
        set => this.RaiseAndSetIfChanged(ref _passed, value);
    }

    private bool _isPending;
    public bool IsPending
    {
        get => _isPending;
        set => this.RaiseAndSetIfChanged(ref _isPending, value);
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set => this.RaiseAndSetIfChanged(ref _isRunning, value);
    }

    private string _preCheckDetails;
    public string PreCheckDetails
    {
        get => _preCheckDetails;
        set => this.RaiseAndSetIfChanged(ref _preCheckDetails, value);
    }

    private bool _actionButtonIsVisible;
    public bool ActionButtonIsVisible
    {
        get => _actionButtonIsVisible;
        set => this.RaiseAndSetIfChanged(ref _actionButtonIsVisible, value);
    }

    private string _actionButtonText;
    public string ActionButtonText
    {
        get => _actionButtonText;
        set => this.RaiseAndSetIfChanged(ref _actionButtonText, value);
    }

    private ICommand _actionButtonCommand;
    public ICommand ActionButtonCommand
    {
        get => _actionButtonCommand;
        set => this.RaiseAndSetIfChanged(ref _actionButtonCommand, value);
    }

    /// <summary>
    /// Base class for pre-checks to run before installation
    /// </summary>
    /// <param name="name">The display name of the pre-check</param>
    /// <param name="required">If installation should stop on failing this pre-check</param>
    public PreCheckBase(string name, bool required)
    {
        Name = name;
        IsRequired = required;
        Id = Guid.NewGuid().ToString();
    }

    public async Task<IResult> RunCheck()
    {
        IsRunning = true;

        var result = await CheckOperation();
        Passed = result.Succeeded;

        PreCheckDetails = !string.IsNullOrWhiteSpace(result.Message)
            ? result.Message
            : (result.Succeeded ? "Pre-Check succeeded, but no details were provided" : "Pre-Check failed, but no details were provided");

        ActionButtonText = result.ActionButtonText;
        ActionButtonCommand = result.ButtonPressedCommand;
        ActionButtonIsVisible = result.ActionButtonIsVisible;

        IsRunning = false;
        IsPending = false;

        return Passed ? Result.FromSuccess() : Result.FromError($"PreCheck Failed: {Name}");
    }

    public abstract Task<PreCheckResult> CheckOperation();
}