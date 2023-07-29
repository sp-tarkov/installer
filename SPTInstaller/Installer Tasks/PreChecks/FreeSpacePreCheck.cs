using System.Linq;
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

    public override async Task<PreCheckResult> CheckOperation()
    {
        if (_internalData.OriginalGamePath is null)
            return PreCheckResult.FromError("Could not find EFT game path");

        if (_internalData.TargetInstallPath is null)
            return PreCheckResult.FromError("Could not find install target path");

        try
        {
            var eftSourceDirectoryInfo = new DirectoryInfo(_internalData.OriginalGamePath);
            var installTargetDirectoryInfo = new DirectoryInfo(_internalData.TargetInstallPath);

            var eftSourceDirSize = DirectorySizeHelper.GetSizeOfDirectory(eftSourceDirectoryInfo);
            var availableSize = DriveInfo.GetDrives().FirstOrDefault(d => d.Name.ToLower() == installTargetDirectoryInfo.Root.Name.ToLower())?.AvailableFreeSpace ?? 0;

            var availableSpaceMessage = $"Available Space: {DirectorySizeHelper.SizeSuffix(availableSize, 2)}";
            var requiredSpaceMessage = $"Space Required for EFT Client: {DirectorySizeHelper.SizeSuffix(eftSourceDirSize, 2)}";

            if (eftSourceDirSize > availableSize)
            {
                return PreCheckResult.FromError($"Not enough free space on {installTargetDirectoryInfo.Root.Name} to install SPT\n\n{availableSpaceMessage}\n{requiredSpaceMessage}");
            }

            return PreCheckResult.FromSuccess($"There is enough space available on {installTargetDirectoryInfo.Root.Name} to install SPT.\n\n{availableSpaceMessage}\n{requiredSpaceMessage}");
        }
        catch (Exception ex)
        {
            return PreCheckResult.FromException(ex);
        }
    }
}