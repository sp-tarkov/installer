using System.Net.Http;
using System.Threading.Tasks;
using Serilog;

namespace SPTInstaller.Helpers;

public static class DownloadCacheHelper
{
    private static HttpClient _httpClient = new() { Timeout = TimeSpan.FromMinutes(15) };

    public static TimeSpan SuggestedTtl = TimeSpan.FromHours(1);
    public static string CachePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "spt-installer/cache");
    
    public static string ReleaseMirrorUrl = "https://spt-releases.modd.in/release.json";
    public static string PatchMirrorUrl = "https://slugma.waffle-lord.net/mirrors.json";
    public static string InstallerUrl = "https://ligma.waffle-lord.net/SPTInstaller.exe";
    public static string InstallerInfoUrl = "https://ligma.waffle-lord.net/installer.json";
    
    public static string GetCacheSizeText()
    {
        if (!Directory.Exists(CachePath))
        {
            var message = "No cache folder";
            Log.Information(message);
            return message;
        }
        
        var cacheDir = new DirectoryInfo(CachePath);
        
        var cacheSize = DirectorySizeHelper.GetSizeOfDirectory(cacheDir);
        
        if (cacheSize == -1)
        {
            var message = "An error occurred while getting the cache size :(";
            Log.Error(message);
            return message;
        }
        
        if (cacheSize == 0)
            return "Empty";
        
        return DirectorySizeHelper.SizeSuffix(cacheSize);
    }


    public static bool ClearMetadataCache()
    {
        var metaData = new DirectoryInfo(CachePath).GetFiles("*.json", SearchOption.TopDirectoryOnly);
        var allDeleted = true;

        foreach (var file in metaData)
        {
            file.Delete();
            file.Refresh();

            if (file.Exists)
            {
                allDeleted = false;
            }
        }
        
        return allDeleted;
    }
    
    /// <summary>
    /// Check if a file in the cache already exists
    /// </summary>
    /// <param name="fileName">The name of the file to check for</param>
    /// <param name="expectedHash">The expected hash of the file in the cache</param>
    /// <param name="cachedFile">The file found in the cache; null if no file is found</param>
    /// <returns>True if the file is in the cache and its hash matches the expected hash, otherwise false</returns>
    public static bool CheckCacheHash(string fileName, string expectedHash, out FileInfo cachedFile)
        => CheckCacheHash(new FileInfo(Path.Join(CachePath, fileName)), expectedHash, out cachedFile);
    
    private static bool CheckCacheHash(FileInfo cacheFile, string expectedHash, out FileInfo fileInCache)
    {
        fileInCache = cacheFile;
        
        try
        {
            cacheFile.Refresh();
            Directory.CreateDirectory(CachePath);
            
            if (!cacheFile.Exists || expectedHash == null)
            {
                Log.Information($"{cacheFile.Name} {(cacheFile.Exists ? "is in cache" : "NOT in cache")}");
                Log.Information($"Expected hash: {(expectedHash == null ? "not provided" : expectedHash)}");
                return false;
            }
            
            if (FileHashHelper.CheckHash(cacheFile, expectedHash))
            {
                fileInCache = cacheFile;
                Log.Information("Hashes MATCH");
                return true;
            }
            
            Log.Warning("Hashes DO NOT MATCH");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Something went wrong during hashing");
            return false;
        }
    }

    /// <summary>
    /// Gets a file in the cache based on a time-to-live from its last modified time
    /// </summary>
    /// <param name="fileName">The name of the file to look for in the cache</param>
    /// <param name="ttl">The time-to-live to check against</param>
    /// <param name="cachedFile">The file found in the cache if it exists</param>
    /// <returns>Returns true if the file was found in the cache, otherwise false</returns>
    public static bool CheckCacheTTL(string fileName, TimeSpan ttl, out FileInfo cachedFile) =>
        CheckCacheTTL(new FileInfo(Path.Join(CachePath, fileName)), ttl, out cachedFile);

    private static bool CheckCacheTTL(FileInfo cacheFile, TimeSpan ttl, out FileInfo fileInCache)
    {
        fileInCache = cacheFile;
        
        try
        {
            cacheFile.Refresh();
            Directory.CreateDirectory(CachePath);
            
            if (!cacheFile.Exists)
            {
                Log.Information($"{cacheFile.Name} {(cacheFile.Exists ? "is in cache" : "NOT in cache")}");
                return false;
            }
            
            var validTimeToLive = cacheFile.LastWriteTime.Add(ttl) > DateTime.Now;

            Log.Information($"{cacheFile.Name} TTL is {(validTimeToLive ? "OK" : "INVALID")}");
            
            return validTimeToLive;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Something went wrong during hashing");
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
    public static async Task<FileInfo?> DownloadFileAsync(string outputFileName, string targetLink,
        IProgress<double> progress)
    {
        Directory.CreateDirectory(CachePath);
        var outputFile = new FileInfo(Path.Join(CachePath, outputFileName));
        
        try
        {
            if (outputFile.Exists)
                outputFile.Delete();
            
            // Use the provided extension method
            using (var file = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                if (!await _httpClient.DownloadDataAsync(targetLink, file, progress))
                {
                    Log.Error($"Download failed: {targetLink}");
                    
                    outputFile.Refresh();
                    
                    if (outputFile.Exists)
                    {
                        outputFile.Delete();
                        return null;
                    }
                }
            }
            
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
        Directory.CreateDirectory(CachePath);
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
            
            outputFile.Refresh();
            
            if (!outputFile.Exists)
            {
                Log.Error("Failed to download file from stream: {name}", outputFileName);
                return null;
            }
            
            return outputFile;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to download file from stream: {fileName}", outputFileName);
            return null;
        }
    }

    /// <summary>
    /// Get or download a file using a time to live
    /// </summary>
    /// <param name="fileName">The file to get from cache</param>
    /// <param name="targetLink">The link to use for the download</param>
    /// <param name="progress">A progress object for reporting download progress</param>
    /// <param name="timeToLive">The time-to-live to check against in the cache</param>
    /// <returns></returns>
    public static async Task<FileInfo?> GetOrDownloadFileAsync(string fileName, string targetLink,
        IProgress<double> progress, TimeSpan timeToLive)
    {
        try
        {
            if (CheckCacheTTL(fileName, timeToLive, out FileInfo cachedFile))
            {
                return cachedFile;
            }

            Log.Information($"Downloading File: {targetLink}");
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
    /// <param name="targetLink">The url to download from if the file doesn't exist in the cache</param>
    /// <param name="progress">A provider for progress updates</param>
    /// <param name="expectedHash">The expected hash of the cached file</param>
    /// <returns>A <see cref="FileInfo"/> object of the cached file</returns>
    /// <remarks>Use <see cref="DownloadFileAsync(string, string, IProgress{double})"/> if you don't have an expected cache file hash</remarks>
    public static async Task<FileInfo?> GetOrDownloadFileAsync(string fileName, string targetLink,
        IProgress<double> progress, string expectedHash)
    {
        try
        {
            if (CheckCacheHash(fileName, expectedHash, out var cacheFile))
                return cacheFile;
            
            Log.Information($"Downloading File: {targetLink}");
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
    public static async Task<FileInfo?> GetOrDownloadFileAsync(string fileName, Stream fileDownloadStream,
        string expectedHash)
    {
        try
        {
            if (CheckCacheHash(fileName, expectedHash, out var cacheFile))
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