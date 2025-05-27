using Serilog;
using SPTInstaller.Helpers;

namespace SPTInstaller.Models;

class CopyInfo(string sourcePath, string targetPath)
{
    public string FileName => $"{Path.GetFileName(sourcePath)}";
    public Result Copy()
    {
        try
        {
            var directory = Path.GetDirectoryName(targetPath);
            Directory.CreateDirectory(directory);
            Log.Debug($"COPY\nSource: {FileHelper.GetRedactedPath(sourcePath)}\nTarget: {FileHelper.GetRedactedPath(targetPath)}");
            File.Copy(sourcePath, targetPath);
            return Result.FromSuccess();
        }
        catch (Exception ex)
        {
            return Result.FromError(ex.Message);
        }
    }
}