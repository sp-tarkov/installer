using SPTInstaller.CustomControls;
using System.Threading.Tasks;

namespace SPTInstaller.Interfaces;

public interface IPreCheck
{
    public string Id { get; }
    public string Name { get; }
    public bool IsRequired { get; }
    public string PreCheckDetails { get; }
    public StatusSpinner.SpinnerState State { get; set; }

    public Task<IResult> RunCheck();
}