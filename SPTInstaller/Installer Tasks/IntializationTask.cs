using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Threading.Tasks;
using SPTInstaller.Helpers;

namespace SPTInstaller.Installer_Tasks;

public class InitializationTask : InstallerTaskBase
{
    private InternalData _data;
    
    public InitializationTask(InternalData data) : base("Startup")
    {
        _data = data;
    }
    
    public override async Task<IResult> TaskOperation()
    {
        SetStatus("Initializing", $"Installed EFT Game Path: {FileHelper.GetRedactedPath(_data.OriginalGamePath)}");
        
        var result = PreCheckHelper.DetectOriginalGameVersion(_data.OriginalGamePath);
        
        if (!result.Succeeded)
        {
            return result;
        }
        
        _data.OriginalGameVersion = result.Message;
        
        SetStatus(null, $"Installed EFT Game Version: {_data.OriginalGameVersion}");
        
        if (_data.OriginalGamePath == null)
        {
            return Result.FromError(
                "Unable to find original EFT directory, please make sure EFT is installed. Please also run EFT once");
        }
        
        if (File.Exists(Path.Join(_data.TargetInstallPath, "EscapeFromTarkov.exe")))
        {
            return Result.FromError(
                "Install location is a folder that has existing game files. Please make the folder doesn't contain an existing spt install");
        }
        
        return Result.FromSuccess($"Current Game Version: {_data.OriginalGameVersion}");
    }
}