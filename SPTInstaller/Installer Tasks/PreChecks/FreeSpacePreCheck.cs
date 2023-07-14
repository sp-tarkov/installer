using System.Threading.Tasks;
using SPTInstaller.Helpers;
using SPTInstaller.Models;

namespace SPTInstaller.Installer_Tasks.PreChecks;

public class FreeSpacePreCheck : PreCheckBase
{
    private readonly InternalData _internalData;

    public FreeSpacePreCheck(InternalData internalData) : base("Free Space", true)
    {
        _internalData = internalData;
    }

    public override async Task<bool> CheckOperation()
    {
        if (_internalData.OriginalGamePath is null || _internalData.TargetInstallPath is null)
        {
            return false;
        }

        return DirectorySizeHelper.CheckAvailableSize(_internalData.OriginalGamePath, _internalData.TargetInstallPath);
    }
}