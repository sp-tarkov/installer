using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Threading.Tasks;
using SPTInstaller.Helpers;

namespace SPTInstaller.Installer_Tasks;

public class CopyClientTask : InstallerTaskBase
{
    private InternalData _data;

    public CopyClientTask(InternalData data) : base("Copy Client Files")
    {
        _data = data;
    }

    public override async Task<IResult> TaskOperation()
    {
        SetStatus("Copying Client Files", "", 0);

        var originalGameDirInfo = new DirectoryInfo(_data.OriginalGamePath);
        var targetInstallDirInfo = new DirectoryInfo(_data.TargetInstallPath);

        return FileHelper.CopyDirectoryWithProgress(originalGameDirInfo, targetInstallDirInfo, (message, progress) => { SetStatus(null, message, progress, null, true); });
    }
}