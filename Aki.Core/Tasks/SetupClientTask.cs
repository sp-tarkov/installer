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
            var targetInstallDirInfo = new DirectoryInfo(_data.TargetInstallPath);

            var patcherOutputDir = new DirectoryInfo(Path.Join(_data.TargetInstallPath, "patcher"));

            var patcherEXE = new FileInfo(Path.Join(_data.TargetInstallPath, "patcher.exe"));

            if (_data.PatchNeeded)
            {
                // extract patcher files
                SetStatus("Extrating Patcher", false);

                var extractPatcherProgress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

                var extractPatcherResult = ZipHelper.Decompress(_data.PatcherZipInfo, patcherOutputDir, extractPatcherProgress);

                if (!extractPatcherResult.Succeeded)
                {
                    return extractPatcherResult;
                }

                // copy patcher files to install directory
                SetStatus("Copying Patcher", false);

                var patcherDirInfo = patcherOutputDir.GetDirectories("Patcher*", SearchOption.TopDirectoryOnly).First();
                

                var copyPatcherProgress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

                var copyPatcherResult = FileHelper.CopyDirectoryWithProgress(patcherDirInfo, targetInstallDirInfo, copyPatcherProgress);

                if (!copyPatcherResult.Succeeded)
                {
                    return copyPatcherResult;
                }

                // run patcher
                SetStatus("Running Patcher");
                StartDrawingIndeterminateProgress();

                var patchingResult = ProcessHelper.PatchClientFiles(patcherEXE, targetInstallDirInfo);

                if (!patchingResult.Succeeded)
                {
                    return patchingResult;
                }
            }

            

            // extract release files
            SetStatus("Extracting Release");
            StartDrawingProgress();

            var extractReleaseProgress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            var extractReleaseResult = ZipHelper.Decompress(_data.AkiZipInfo, targetInstallDirInfo, extractReleaseProgress);

            if (!extractReleaseResult.Succeeded)
            {
                return extractReleaseResult;
            }

            // cleanup temp files
            SetStatus("Cleanup");
            StartDrawingIndeterminateProgress();

            if(_data.PatchNeeded)
            {
                patcherOutputDir.Delete(true);
                patcherEXE.Delete();
            }

            return GenericResult.FromSuccess("SPT is Setup. Happy Playing!");
        }
    }
}
