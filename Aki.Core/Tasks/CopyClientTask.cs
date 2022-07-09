using SPT_AKI_Installer.Aki.Core.Model;
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

            try
            {
                int totalFiles = Directory.GetFiles(_data.OriginalGamePath, "*.*", SearchOption.AllDirectories).Length;
                int processedFiles = 0;

                foreach (string dirPath in Directory.GetDirectories(_data.OriginalGamePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(_data.OriginalGamePath, _data.TargetInstallPath));
                }

                foreach (string newPath in Directory.GetFiles(_data.OriginalGamePath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(_data.OriginalGamePath, _data.TargetInstallPath), true);
                    processedFiles++;

                    Progress = (int)Math.Floor(((double)processedFiles / totalFiles) * 100);
                }

                return GenericResult.FromSuccess();
            }
            catch(Exception ex)
            {
                return GenericResult.FromError(ex.Message);
            }
        }
    }
}
