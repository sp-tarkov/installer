using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SPT_AKI_Installer.Aki.Core.Model;
using System;
using System.IO;
using System.Linq;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class ZipHelper
    {
        public static GenericResult Decompress(FileInfo ArchivePath, DirectoryInfo OutputFolderPath, IProgress<double> progress = null)
        {
            try
            {
                OutputFolderPath.Refresh();

                if (!OutputFolderPath.Exists) OutputFolderPath.Create();

                using var archive = ZipArchive.Open(ArchivePath);
                var totalEntries = archive.Entries.Where(entry => !entry.IsDirectory);
                int processedEntries = 0;

                foreach (var entry in totalEntries)
                {
                    entry.WriteToDirectory(OutputFolderPath.FullName, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });

                    processedEntries++;

                    if (progress != null)
                    {
                        progress.Report(Math.Floor(((double)processedEntries / totalEntries.Count()) * 100));
                    }
                }

                OutputFolderPath.Refresh();

                if (!OutputFolderPath.Exists)
                {
                    return GenericResult.FromError($"Failed to extract files: {ArchivePath.Name}");
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
