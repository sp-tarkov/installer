using SPT_AKI_Installer.Aki.Core.Model;
using SPT_AKI_Installer.Aki.Helper;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Core.Tasks
{
    public class SetupClientTask : LiveTableTask
    {
        private InternalData _data;

        public SetupClientTask(InternalData data) : base("Setup Client", false)
        {
            _data = data;
        }

        public override async Task<GenericResult> RunAsync()
        {
            // extract patcher files
            SetStatus("Extrating Patcher", false);

            var extractPatcherProgress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            var patcherOutputDir = new DirectoryInfo(Path.Join(_data.TargetInstallPath, "patcher"));

            var extractPatcherResult = ZipHelper.Decompress(_data.PatcherZipInfo, patcherOutputDir, extractPatcherProgress);

            if(!extractPatcherResult.Succeeded)
            {
                return extractPatcherResult;
            }

            // copy patcher files to install directory
            SetStatus("Copying Patcher", false);

            var patcherDirInfo = patcherOutputDir.GetDirectories("Patcher*", SearchOption.TopDirectoryOnly).First();
            var targetInstallDirInfo = new DirectoryInfo(_data.TargetInstallPath);

            var copyPatcherProgress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            var copyPatcherResult = FileHelper.CopyDirectoryWithProgress(patcherDirInfo, targetInstallDirInfo, copyPatcherProgress);

            if(!copyPatcherResult.Succeeded)
            {
                return copyPatcherResult;
            }

            // run patcher
            SetStatus("Running Patcher");
            StartDrawingIndeterminateProgress();

            var patcherEXE = new FileInfo(Path.Join(_data.TargetInstallPath, "patcher.exe"));

            var patchingResult = ProcessHelper.PatchClientFiles(patcherEXE, targetInstallDirInfo);

            if(!patchingResult.Succeeded)
            {
                return patchingResult;
            }

            // extract release files
            SetStatus("Extracting Release");
            StartDrawingProgress();

            var extractReleaseProgress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            var extractReleaseResult = ZipHelper.Decompress(_data.AkiZipInfo, targetInstallDirInfo, extractReleaseProgress);

            if(!extractReleaseResult.Succeeded)
            {
                return extractPatcherResult;
            }

            // cleanup temp files
            SetStatus("Cleanup");
            StartDrawingIndeterminateProgress();

            patcherOutputDir.Delete(true);
            _data.PatcherZipInfo.Delete();
            _data.AkiZipInfo.Delete();
            patcherEXE.Delete();

            return GenericResult.FromSuccess("SPT is Setup. Happy Playing!");
        }
    }
}
