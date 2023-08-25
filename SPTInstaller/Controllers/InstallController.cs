using Serilog;
using SharpCompress;
using SPTInstaller.CustomControls;
using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPTInstaller.Controllers;

public class InstallController
{
    public event EventHandler<IProgressableTask> TaskChanged = delegate { };

    private IPreCheck[] _preChecks { get; set; }
    private IProgressableTask[] _tasks { get; set; }

    public InstallController(IProgressableTask[] tasks, IPreCheck[] preChecks = null)
    {
        _tasks = tasks;
        _preChecks = preChecks;
    }

    public async Task<IResult> RunPreChecks()
    {
        Log.Information("-<>--<>- Running PreChecks -<>--<>-");
        var requiredResults = new List<IResult>();

        _preChecks.ForEach(x => x.State = StatusSpinner.SpinnerState.Pending);

        foreach (var check in _preChecks)
        {
            var result = await check.RunCheck();

            Log.Information($"PreCheck: {check.Name} ({(check.IsRequired ? "Required" : "Optional")}) -> {(result.Succeeded ? "Passed" : "Failed")}\nDetail: {check.PreCheckDetails.ReplaceLineEndings(" ")}");
            
            if (check.IsRequired)
            {
                requiredResults.Add(result);
            }
        }

        if (requiredResults.Any(result => !result.Succeeded))
        {
            return Result.FromError("Some required checks have failed");
        }

        return Result.FromSuccess();
    }

    public async Task<IResult> RunTasks()
    {
        Log.Information("-<>--<>- Running Installer Tasks -<>--<>-");

        foreach (var task in _tasks)
        {
            TaskChanged?.Invoke(null, task);

            var result = await task.RunAsync();

            if (!result.Succeeded) return result;
        }

        return Result.FromSuccess("Install Complete. Happy Playing!");
    }
}