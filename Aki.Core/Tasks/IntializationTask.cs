using SPT_AKI_Installer.Aki.Core.Model;
using SPT_AKI_Installer.Aki.Helper;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Core.Tasks
{
    public class InitializationTask : LiveTableTask
    {
        private InternalData _data;

        public InitializationTask(InternalData data) : base("Startup")
        {
            _data = data;
        }

        public override async Task<GenericResult> RunAsync()
        {
            _data.OriginalGamePath = PreCheckHelper.DetectOriginalGamePath();

            if (_data.OriginalGamePath == null)
            {
                return GenericResult.FromError("EFT IS NOT INSTALLED!");
            }

            _data.OriginalGameVersion = PreCheckHelper.DetectOriginalGameVersion(_data.OriginalGamePath);

            if (_data.OriginalGamePath == null)
            {
                return GenericResult.FromError("Unable to find EFT OG directory, please make sure EFT is installed. Please also run EFT once");
            }

            if (_data.OriginalGamePath == _data.TargetInstallPath)
            {
                return GenericResult.FromError("Installer is located in EFT's original directory. Please move the installer to a seperate folder as per the guide");
            }

            if (File.Exists(Path.Join(_data.TargetInstallPath, "EscapeFromTarkov.exe")))
            {
                return GenericResult.FromError("Installer is located in a Folder that has existing Game Files. Please make sure the installer is in a fresh folder as per the guide");
            }

            return GenericResult.FromSuccess($"Current Game Version: {_data.OriginalGameVersion}");
        }
    }
}
