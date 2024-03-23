using SevenZip;
using SPTInstaller.Models;

namespace SPTInstaller.Helpers;

public static class ZipHelper
{
    public static Result Decompress(FileInfo archiveFile, DirectoryInfo outputDirectory, IProgress<double> progress = null)
    {
        try
        {
            using var archiveStream = archiveFile.OpenRead();

            var dllPath = Path.Join(DownloadCacheHelper.CachePath, "7z.dll");
            
            SevenZipBase.SetLibraryPath(dllPath);

            var extractor = new SevenZipExtractor(archiveStream);

            extractor.Extracting += (_, args) =>
            {
                progress.Report(args.PercentDone);
            };

            extractor.ExtractArchive(outputDirectory.FullName);

            outputDirectory.Refresh();

            if (!outputDirectory.Exists)
            {
                return Result.FromError($"Failed to extract files: {archiveFile.Name}");
            }

            return Result.FromSuccess();
        }
        catch (Exception ex)
        {
            return Result.FromError(ex.Message);
        }
    }
}