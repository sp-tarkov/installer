using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Linq;
using System.Threading.Tasks;
using SPTInstaller.Helpers;

namespace SPTInstaller.Installer_Tasks;

public class SetupClientTask : InstallerTaskBase
{
    private InternalData _data;
    
    public SetupClientTask(InternalData data) : base("Setup Client")
    {
        _data = data;
    }
    
    public override async Task<IResult> TaskOperation()
    {
        var targetInstallDirInfo = new DirectoryInfo(_data.TargetInstallPath);
        
        var patcherOutputDir = new DirectoryInfo(Path.Join(_data.TargetInstallPath, "patcher"));
        
        var patcherEXE = new FileInfo(Path.Join(_data.TargetInstallPath, "patcher.exe"));
        
        var progress = new Progress<double>((d) => { SetStatus(null, null, (int)Math.Floor(d)); });
        
        SetStatus("Preparing 7z", "", null, ProgressStyle.Indeterminate);
        
        if (!FileHelper.StreamAssemblyResourceOut("7z.dll", Path.Join(DownloadCacheHelper.CachePath, "7z.dll")))
        {
            return Result.FromError("Failed to prepare 7z");
        }
        
        if (_data.PatchNeeded)
        {
            // extract patcher files
            SetStatus("Extracting Patcher", "", 0);
            
            var extractPatcherResult = ZipHelper.Decompress(_data.PatcherZipInfo, patcherOutputDir, progress);
            
            if (!extractPatcherResult.Succeeded)
            {
                return extractPatcherResult;
            }
            
            // copy patcher files to install directory
            SetStatus("Copying Patcher", "", 0);
            
            var patcherDirInfo = patcherOutputDir.GetDirectories("Patcher*", SearchOption.TopDirectoryOnly).First();
            
            var copyPatcherResult =
                FileHelper.CopyDirectoryWithProgress(patcherDirInfo, targetInstallDirInfo, progress);
            
            if (!copyPatcherResult.Succeeded)
            {
                return copyPatcherResult;
            }
            
            // run patcher
            SetStatus("Running Patcher", "", null, ProgressStyle.Indeterminate);
            
            var patchingResult = ProcessHelper.PatchClientFiles(patcherEXE, targetInstallDirInfo);
            
            if (!patchingResult.Succeeded)
            {
                return patchingResult;
            }
        }
        
        // extract release files
        SetStatus("Extracting Release", "", 0);
        
        var extractReleaseResult = ZipHelper.Decompress(_data.SPTZipInfo, targetInstallDirInfo, progress);
        
        if (!extractReleaseResult.Succeeded)
        {
            return extractReleaseResult;
        }

        SetStatus("Creating Shortcuts", "", 0);

        var sptPath = $"{Path.Join(_data.TargetInstallPath, "SPT")}";
        var shortcutResult = ProcessHelper.RunEmbeddedScript("add_shortcuts.ps1", _data.TargetInstallPath, sptPath);
        if (!shortcutResult.Succeeded)
        {
            return shortcutResult;
        }

        // cleanup temp files
        SetStatus("Cleanup", "almost done :)", null, ProgressStyle.Indeterminate);
        
        if (_data.PatchNeeded)
        {
            if (patcherOutputDir.Exists)
            {
                patcherOutputDir.Delete(true);
            }

            if (patcherEXE.Exists)
            {
                patcherEXE.Delete();
            }
        }
        
        return Result.FromSuccess("SPT is Setup. Happy Playing!");
    }
}