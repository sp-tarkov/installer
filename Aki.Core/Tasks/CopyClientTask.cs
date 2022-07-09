using SPT_AKI_Installer.Aki.Core.Model;
using SPT_AKI_Installer.Aki.Helper;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Core.Tasks
{
    public class CopyClientTask : LiveTableTask
    {
        private InternalData _data;

        public CopyClientTask(InternalData data) : base("Copy Client Files", false)
        {
            _data = data;
        }

        public override async Task<GenericResult> RunAsync()
        {
            SetStatus("Copying", false);

            var originalGameDirInfo = new DirectoryInfo(_data.OriginalGamePath);
            var targetInstallDirInfo = new DirectoryInfo(_data.TargetInstallPath);

            var progress = new Progress<double>((d) => { Progress = (int)Math.Floor(d); });

            return FileHelper.CopyDirectoryWithProgress(originalGameDirInfo, targetInstallDirInfo, progress);
        }
    }
}
