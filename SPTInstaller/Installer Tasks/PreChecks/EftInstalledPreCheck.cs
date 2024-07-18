using System.Threading.Tasks;
using SPTInstaller.Models;

namespace SPTInstaller.Installer_Tasks.PreChecks;

public class EftInstalledPreCheck : PreCheckBase
{
    private InternalData _internalData;
    
    public EftInstalledPreCheck(InternalData data) : base("EFT Installed", true)
    {
        _internalData = data;
    }
    
    public override async Task<PreCheckResult> CheckOperation()
    {
        if (_internalData.OriginalGamePath is null || !Directory.Exists(_internalData.OriginalGamePath) || !File.Exists(Path.Join(_internalData.OriginalGamePath, "Escapefromtarkov.exe")))
        {
            return PreCheckResult.FromError("Your EFT installation could not be found, try running the Battlestate games launcher and ensure EFT is installed on your computer", "Retry", RequestReevaluation);
        }
        
        return PreCheckResult.FromSuccess("EFT install folder found");
    }
}