using HttpClientProgress;
using Serilog;
using SPTInstaller.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPTInstaller.Aki.Helper
{
    public static class DownloadCacheHelper
    {
        private static HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromHours(1) };

        private static string _cachePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "spt-installer/cache");

        private static bool CheckCache(FileInfo cacheFile, string expectedHash = null)
        {
            try
            {
                cacheFile.Refresh();
                Directory.CreateDirectory(_cachePath);

                if (cacheFile.Exists)
                {
                    if (expectedHash != null && FileHashHelper.CheckHash(cacheFile, expectedHash))
                    {
                        Log.Information($"Using cached file: {cacheFile.Name}");
                        return true;
                    }

                    cacheFile.Delete();
                    cacheFile.Refresh();
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<Result> DownloadFile(FileInfo outputFile, string targetLink, IProgress<double> progress, string expectedHash = null)
        {
            try
            {
                // Use the provided extension method
                using (var file = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
                    await _httpClient.DownloadDataAsync(targetLink, file, progress);

                outputFile.Refresh();

                if (!outputFile.Exists)
                {
                    return Result.FromError($"Failed to download {outputFile.Name}");
                }

                if (expectedHash != null && !FileHashHelper.CheckHash(outputFile, expectedHash))
                {
                    return Result.FromError("Hash mismatch");
                }

                return Result.FromSuccess();
            }
            catch (Exception ex)
            {
                return Result.FromError(ex.Message);
            }
        }

        private static async Task<Result> ProcessInboundStreamAsync(FileInfo cacheFile, Stream downloadStream, string expectedHash = null)
        {
            try
            {
                if (CheckCache(cacheFile, expectedHash)) return Result.FromSuccess();

                using var patcherFileStream = cacheFile.Open(FileMode.Create);
                {
                    await downloadStream.CopyToAsync(patcherFileStream);
                }

                patcherFileStream.Close();

                if (expectedHash != null && !FileHashHelper.CheckHash(cacheFile, expectedHash))
                {
                    return Result.FromError("Hash mismatch");
                }

                return Result.FromSuccess();
            }
            catch(Exception ex)
            {
                return Result.FromError(ex.Message);
            }
        }

        private static async Task<Result> ProcessInboundFileAsync(FileInfo cacheFile, string targetLink, IProgress<double> progress, string expectedHash = null)
        {
            try
            {
                if (CheckCache(cacheFile, expectedHash)) return Result.FromSuccess();

                return await DownloadFile(cacheFile, targetLink, progress, expectedHash);
            }
            catch(Exception ex)
            {
                return Result.FromError(ex.Message);
            }
        }

        public static async Task<FileInfo?> GetOrDownloadFileAsync(string fileName, string targetLink, IProgress<double> progress, string expectedHash = null)
        {
            FileInfo cacheFile = new FileInfo(Path.Join(_cachePath, fileName));

            try
            {
                var result = await ProcessInboundFileAsync(cacheFile, targetLink, progress, expectedHash);

                return result.Succeeded ? cacheFile : null;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<FileInfo?> GetOrDownloadFileAsync(string fileName, Stream fileDownloadStream, string expectedHash = null)
        {
            FileInfo cacheFile = new FileInfo(Path.Join(_cachePath, fileName));

            try
            {
                var result = await ProcessInboundStreamAsync(cacheFile, fileDownloadStream, expectedHash);

                return result.Succeeded ? cacheFile : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
