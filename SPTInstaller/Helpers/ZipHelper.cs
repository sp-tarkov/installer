using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SPTInstaller.Models;

namespace SPTInstaller.Helpers;

public static class ZipHelper
{
    public static Result Decompress(FileInfo ArchivePath, DirectoryInfo OutputFolderPath, IProgress<double> progress = null)
    {
        try
        {
            OutputFolderPath.Refresh();

            if (!OutputFolderPath.Exists) OutputFolderPath.Create();

            using var archive = ZipArchive.Open(ArchivePath);
            var totalEntries = archive.Entries.Where(entry => !entry.IsDirectory);
            var processedEntries = 0;

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
                return Result.FromError($"Failed to extract files: {ArchivePath.Name}");
            }

            return Result.FromSuccess();
        }
        catch (Exception ex)
        {
            return Result.FromError(ex.Message);
        }
    }
}