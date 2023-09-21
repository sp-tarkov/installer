using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using SPTInstaller.Models;

namespace SPTInstaller.Helpers;

public static class DownloadCacheHelper
{
    private static HttpClient _httpClient = new() { Timeout = TimeSpan.FromHours(1) };

    public static string CachePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "spt-installer/cache");

    public static string GetCacheSizeText()
    {
        if (!Directory.Exists(CachePath))
            return "No cache folder";

        var cacheDir = new DirectoryInfo(CachePath);

        var cacheSize = DirectorySizeHelper.GetSizeOfDirectory(cacheDir);

        if (cacheSize == 0)
            return "Empty";

        return DirectorySizeHelper.SizeSuffix(cacheSize);
    }

    /// <summary>
    /// Check if a file in the cache already exists
    /// </summary>
    /// <param name="fileName">The name of the file to check for</param>
    /// <param name="expectedHash">The expected hash of the file in the cache</param>
    /// <param name="cachedFile">The file found in the cache; null if no file is found</param>
    /// <returns>True if the file is in the cache and its hash matches the expected hash, otherwise false</returns>
    public static bool CheckCache(string fileName, string expectedHash, out FileInfo cachedFile) 
        => CheckCache(new FileInfo(Path.Join(CachePath, fileName)), expectedHash, out cachedFile);

    private static bool CheckCache(FileInfo cacheFile, string expectedHash, out FileInfo fileInCache)
    {
        fileInCache = cacheFile;

        try
        {
            cacheFile.Refresh();
            Directory.CreateDirectory(CachePath);

            if (!cacheFile.Exists || expectedHash == null)
                return false;

            if (FileHashHelper.CheckHash(cacheFile, expectedHash))
            {
                fileInCache = cacheFile;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Download a file to the cache folder
    /// </summary>
    /// <param name="outputFileName">The file name to save the file as</param>
    /// <param name="targetLink">The url to download the file from</param>
    /// <param name="progress">A provider for progress updates</param>
    /// <returns>A <see cref="FileInfo"/> object of the cached file</returns>
    /// <remarks>If the file exists, it is deleted before downloading</remarks>
    public static async Task<FileInfo?> DownloadFileAsync(string outputFileName, string targetLink, IProgress<double> progress)
    {
        var outputFile = new FileInfo(Path.Join(CachePath, outputFileName));

        try
        {
            if (outputFile.Exists)
                outputFile.Delete();

            // Use the provided extension method
            using (var file = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
                await _httpClient.DownloadDataAsync(targetLink, file, progress);

            outputFile.Refresh();

            if (!outputFile.Exists)
            {
                Log.Error("Failed to download file from url: {name} :: {url}", outputFileName, targetLink);
                return null;
            }

            return outputFile;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to download file from url: {name} :: {url}", outputFileName, targetLink);
            return null;
        }
    }

    /// <summary>
    /// Download a file to the cache folder
    /// </summary>
    /// <param name="outputFileName">The file name to save the file as</param>
    /// <param name="downloadStream">The stream the download the file from</param>
    /// <returns>A <see cref="FileInfo"/> object of the cached file</returns>
    /// <remarks>If the file exists, it is deleted before downloading</remarks>
    public static async Task<FileInfo?> DownloadFileAsync(string outputFileName, Stream downloadStream)
    {
        var outputFile = new FileInfo(Path.Join(CachePath, outputFileName));

        try
        {
            if (outputFile.Exists)
                outputFile.Delete();

            using var patcherFileStream = outputFile.Open(FileMode.Create);
            {
                await downloadStream.CopyToAsync(patcherFileStream);
            }

            patcherFileStream.Close();

            if (!outputFile.Exists)
            {
                Log.Error("Failed to download file from stream: {name}", outputFileName);
                return null;
            }

            return outputFile;
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Failed to download file from stream: {fileName}", outputFileName);
            return null;
        }
    }

    /// <summary>
    /// Get the file from cache or download it
    /// </summary>
    /// <param name="fileName">The name of the file to check for in the cache</param>
    /// <param name="targetLink">The url to download from if the file doesn't exist in the cache</param>
    /// <param name="progress">A provider for progress updates</param>
    /// <param name="expectedHash">The expected hash of the cached file</param>
    /// <returns>A <see cref="FileInfo"/> object of the cached file</returns>
    /// <remarks>Use <see cref="DownloadFileAsync(string, string, IProgress{double})"/> if you don't have an expected cache file hash</remarks>
    public static async Task<FileInfo?> GetOrDownloadFileAsync(string fileName, string targetLink, IProgress<double> progress, string expectedHash)
    {
        try
        {
            if (CheckCache(fileName, expectedHash, out var cacheFile))
                return cacheFile;

            return await DownloadFileAsync(fileName, targetLink, progress);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error while getting file: {fileName}");
            return null;
        }
    }

    /// <summary>
    /// Get the file from cache or download it
    /// </summary>
    /// <param name="fileName">The name of the file to check for in the cache</param>
    /// <param name="fileDownloadStream">The stream to download from if the file doesn't exist in the cache</param>
    /// <param name="expectedHash">The expected hash of the cached file</param>
    /// <returns>A <see cref="FileInfo"/> object of the cached file</returns>
    /// <remarks>Use <see cref="DownloadFileAsync(string, Stream)"/> if you don't have an expected cache file hash</remarks>
    public static async Task<FileInfo?> GetOrDownloadFileAsync(string fileName, Stream fileDownloadStream, string expectedHash)
    {
        try
        {
            if (CheckCache(fileName, expectedHash, out var cacheFile))
                return cacheFile;

            return await DownloadFileAsync(fileName, fileDownloadStream);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error while getting file: {fileName}");
            return null;
        }
    }
}