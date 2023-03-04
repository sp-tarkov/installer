using HttpClientProgress;
using SPT_AKI_Installer.Aki.Core.Model;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class DownloadCacheHelper
    {
        private static HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromHours(1) };

        private static string _cachePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "spt-installer/cache");

        private static async Task<GenericResult> DownloadFile(FileInfo outputFile, string targetLink, IProgress<double> progress, string expectedHash = null)
        {
            try
            {
                // Use the provided extension method
                using (var file = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
                    await _httpClient.DownloadDataAsync(targetLink, file, progress);

                outputFile.Refresh();

                if (!outputFile.Exists)
                {
                    return GenericResult.FromError($"Failed to download {outputFile.Name}");
                }

                if (expectedHash != null && !FileHashHelper.CheckHash(outputFile, expectedHash))
                {
                    return GenericResult.FromError("Hash mismatch");
                }

                return GenericResult.FromSuccess();
            }
            catch (Exception ex)
            {
                return GenericResult.FromError(ex.Message);
            }
        }

        private static async Task<GenericResult> ProcessInboundStreamAsync(FileInfo cacheFile, Stream downloadStream, string expectedHash = null)
        {
            if (cacheFile.Exists)
            {
                if (expectedHash != null && FileHashHelper.CheckHash(cacheFile, expectedHash))
                {
                    return GenericResult.FromSuccess();
                }

                cacheFile.Delete();
                cacheFile.Refresh();
            }

            using var patcherFileStream = cacheFile.Open(FileMode.Create);
            {
                await downloadStream.CopyToAsync(patcherFileStream);
            }

            patcherFileStream.Close();

            if (expectedHash != null && !FileHashHelper.CheckHash(cacheFile, expectedHash))
            {
                return GenericResult.FromError("Hash mismatch");
            }

            return GenericResult.FromSuccess();
        }

        private static async Task<GenericResult> ProcessInboundFileAsync(FileInfo cacheFile, string targetLink, IProgress<double> progress, string expectedHash = null)
        {
            try
            {
                cacheFile.Refresh();
                Directory.CreateDirectory(_cachePath);

                if (cacheFile.Exists)
                {
                    if (expectedHash != null && FileHashHelper.CheckHash(cacheFile, expectedHash))
                    {
                        return GenericResult.FromSuccess();
                    }

                    cacheFile.Delete();
                    cacheFile.Refresh();
                }

                return await DownloadFile(cacheFile, targetLink, progress, expectedHash);
            }
            catch(Exception ex)
            {
                return GenericResult.FromError(ex.Message);
            }
        }

        public static async Task<FileInfo> GetOrDownloadFileAsync(string fileName, string targetLink, IProgress<double> progress, string expectedHash = null)
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

        public static async Task<FileInfo> GetOrDownloadFileAsync(string fileName, Stream fileDownloadStream, string expectedHash = null)
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
