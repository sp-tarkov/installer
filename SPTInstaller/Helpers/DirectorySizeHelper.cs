using System.Linq;
using Serilog;

namespace SPTInstaller.Helpers;

public static class DirectorySizeHelper
{
    public static bool CheckAvailableSize(string eftSourceDirPath, string installTargetDirPath)
    {
        try
        {
            var eftSourceDirectoryInfo = new DirectoryInfo(eftSourceDirPath);
            var installTargetDirectoryInfo = new DirectoryInfo(installTargetDirPath);
                
            var eftSourceDirSize = GetSizeOfDirectory(eftSourceDirectoryInfo);
            var availableSize = DriveInfo.GetDrives().FirstOrDefault(d => d.Name.ToLower() == installTargetDirectoryInfo.Root.Name.ToLower())?.AvailableFreeSpace ?? 0;

            return eftSourceDirSize < availableSize;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while checking available size");

            return false;
        }
    }
        
    private static long GetSizeOfDirectory(DirectoryInfo sourceDir) => sourceDir.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
}