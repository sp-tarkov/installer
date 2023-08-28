using ReactiveUI;
using SPTInstaller.CustomControls;
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

    private StatusSpinner.SpinnerState _state;
    public StatusSpinner.SpinnerState State
    {
        get => _state;
        set => this.RaiseAndSetIfChanged(ref _state, value);
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

    private StatusSpinner.SpinnerState ProcessResult(PreCheckResult result) =>
        (result.Succeeded, IsRequired) switch
        {
            (true, _)      => StatusSpinner.SpinnerState.OK,
            (false, false) => StatusSpinner.SpinnerState.Warning,
            (_, _)         => StatusSpinner.SpinnerState.Error
        };

    public async Task<IResult> RunCheck()
    {
        State = StatusSpinner.SpinnerState.Running;

        var result = await CheckOperation();

        PreCheckDetails = !string.IsNullOrWhiteSpace(result.Message)
            ? result.Message
            : (result.Succeeded ? "Pre-Check succeeded, but no details were provided" : "Pre-Check failed, but no details were provided");

        ActionButtonText = result.ActionButtonText;
        ActionButtonCommand = result.ButtonPressedCommand;
        ActionButtonIsVisible = result.ActionButtonIsVisible;

        State = ProcessResult(result);

        return State == StatusSpinner.SpinnerState.OK ? Result.FromSuccess() : Result.FromError($"PreCheck Failed: {Name}");
    }

    public abstract Task<PreCheckResult> CheckOperation();
}