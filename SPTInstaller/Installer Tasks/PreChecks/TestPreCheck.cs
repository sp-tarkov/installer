using SPTInstaller.CustomControls;
using SPTInstaller.Models;
using System.Threading.Tasks;

namespace SPTInstaller.Installer_Tasks.PreChecks;
public class TestPreCheck : PreCheckBase
{
    private StatusSpinner.SpinnerState _endState;
    public static TestPreCheck FromRandomName(StatusSpinner.SpinnerState EndState) => new TestPreCheck($"{EndState} #{new Random().Next(0, 9999)}", EndState == StatusSpinner.SpinnerState.Error, EndState);

    public TestPreCheck(string name, bool isRequired, StatusSpinner.SpinnerState endState) : base(name, isRequired)
    {
        _endState = endState;
    }

    public override async Task<PreCheckResult> CheckOperation()
    {
        await Task.Delay(1000);

        switch (_endState)
        {
            case StatusSpinner.SpinnerState.Error:
                return PreCheckResult.FromError("This is what a required precheck failing looks like");
            case StatusSpinner.SpinnerState.Warning:
                return PreCheckResult.FromError("This is what a non-required precheck failing looks like");
            default:
                return PreCheckResult.FromSuccess("This is what a successful precheck looks like");
        }
    }
}
