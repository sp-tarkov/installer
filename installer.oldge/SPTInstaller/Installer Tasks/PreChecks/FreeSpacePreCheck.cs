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
            
            var cacheDirectory = new DirectoryInfo(DownloadCacheHelper.CachePath);
            
            var eftSourceDirSize = DirectorySizeHelper.GetSizeOfDirectory(eftSourceDirectoryInfo);
            
            if (eftSourceDirSize == -1)
            {
                return PreCheckResult.FromError("An error occurred while getting the EFT source directory size. This is most likely because EFT is not installed");
            }
            
            var availableSize = DriveInfo.GetDrives()
                .FirstOrDefault(d => d.Name.ToLower() == installTargetDirectoryInfo.Root.Name.ToLower())
                ?.AvailableFreeSpace ?? 0;
            
            // add 10Gb overhead to game files for potential patches / release files
            eftSourceDirSize += 10000000000;
            
            var availableSpaceMessage = $"Available Space: {DirectorySizeHelper.SizeSuffix(availableSize, 2)}";
            var requiredSpaceMessage =
                $"Space Required for EFT Client: {DirectorySizeHelper.SizeSuffix(eftSourceDirSize, 2)} including ~10Gb overhead";
            
            var cacheDriveMessage = "";
            var cacheDriveOK = true;
            
            // if cache directory is on another drive, check that drive for around 5Gb of required space
            if (cacheDirectory.Root.Name.ToLower() != installTargetDirectoryInfo.Root.Name.ToLower())
            {
                cacheDriveOK = false;
                var availableCacheDriveSize = DriveInfo.GetDrives()
                                              .FirstOrDefault(d =>
                                                  d.Name.ToLower() == cacheDirectory.Root.Name.ToLower())
                                              ?.AvailableFreeSpace ??
                                          0;
                
                // check if the drive where the cache is has at least 5Gb of free space. We should only need 2-3Gb
                if (availableCacheDriveSize > 5000000000)
                {
                    cacheDriveMessage = $"Drive for cache '{cacheDirectory.Root.Name}' has at least 5Gb of space. Available: {DirectorySizeHelper.SizeSuffix(availableCacheDriveSize, 2)}";
                    cacheDriveOK = true;
                }
                else
                {
                    cacheDriveMessage = $"Drive for cache '{cacheDirectory.Root.Name}' does NOT have at least 5Gb of space. Available: {DirectorySizeHelper.SizeSuffix(availableCacheDriveSize, 2)}";
                }
            }
            
            if (eftSourceDirSize > availableSize)
            {
                return PreCheckResult.FromError(
                    $"Not enough free space on {installTargetDirectoryInfo.Root.Name} to install SPT\n\n{availableSpaceMessage}\n{requiredSpaceMessage}\n\n{cacheDriveMessage}");
            }
            
            var okGameSpaceMessage =
                $"There is enough space available on {installTargetDirectoryInfo.Root.Name} to install SPT.\n\n{availableSpaceMessage}\n{requiredSpaceMessage}\n\n{cacheDriveMessage}";
            
            if (!cacheDriveOK)
            {
                return PreCheckResult.FromError(okGameSpaceMessage);
            }
            
            return PreCheckResult.FromSuccess(okGameSpaceMessage);
        }
        catch (Exception ex)
        {
            return PreCheckResult.FromException(ex);
        }
    }
}