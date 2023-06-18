using System;
using System.IO;
using System.Linq;
using Serilog;
using SPTInstaller.Models;

namespace SPTInstaller.Helpers
{
    public static class DirectorySizeHelper
    {
        public static Result CheckAvailableSize(string eftSourceDirPath, string installTargetDirPath)
        {
            try
            {
                var eftSourceDirectoryInfo = new DirectoryInfo(eftSourceDirPath);
                var installTargetDirectoryInfo = new DirectoryInfo(installTargetDirPath);
                
                var eftSourceDirSize = GetSizeOfDirectory(eftSourceDirectoryInfo);
                var availableSize = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == installTargetDirectoryInfo.Root.Name)?.AvailableFreeSpace ?? 0;

                if (eftSourceDirSize > availableSize)
                {
                    return Result.FromError($"Not enough space on drive {installTargetDirectoryInfo.Root.Name}.\n\nRequired: {FormatFileSize(eftSourceDirSize)}\nAvailable: {FormatFileSize(availableSize)}");
                }

                return Result.FromSuccess();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking available size");

                return Result.FromError(ex.Message);
            }
        }
        
        private static long GetSizeOfDirectory(DirectoryInfo sourceDir) => sourceDir.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);

        private static string FormatFileSize(long bytes)
        {
            const int unit = 1024;
            var exp = (int)(Math.Log(bytes) / Math.Log(unit));

            return $"{bytes / Math.Pow(unit, exp):F2} {"KMGTPE"[exp - 1]}B";
        }
    }
}