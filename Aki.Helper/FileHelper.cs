using SPT_AKI_Installer.Aki.Core.Model;
using System;
using System.IO;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class FileHelper
    {
        public static GenericResult CopyDirectoryWithProgress(DirectoryInfo sourceDir, DirectoryInfo targetDir, IProgress<double> progress)
        {
            try
            {
                int totalFiles = sourceDir.GetFiles("*.*", SearchOption.AllDirectories).Length;
                int processedFiles = 0;

                foreach (var dir in sourceDir.GetDirectories("*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dir.FullName.Replace(sourceDir.FullName, targetDir.FullName));
                }

                foreach (var file in sourceDir.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    File.Copy(file.FullName, file.FullName.Replace(sourceDir.FullName, targetDir.FullName), true);
                    processedFiles++;

                    progress.Report((int)Math.Floor(((double)processedFiles / totalFiles) * 100));
                }

                return GenericResult.FromSuccess();
            }
            catch (Exception ex)
            {
                return GenericResult.FromError(ex.Message);
            }
        }
    }
}
