using HttpClientProgress;
using SPT_AKI_Installer.Aki.Core.Model;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class DownloadHelper
    {
        private static HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromHours(1) };

        public static bool FileHashCheck(FileInfo file, string expectedHash)
        {
            using (MD5 md5Service = MD5.Create())
            using (var sourceStream = file.OpenRead())
            {
                byte[] sourceHash = md5Service.ComputeHash(sourceStream);
                byte[] expectedHashBytes = Convert.FromBase64String(expectedHash);

                bool matched = Enumerable.SequenceEqual(sourceHash, expectedHashBytes);

                return matched;
            }
        }


        public static async Task<GenericResult> DownloadFile(FileInfo outputFile, string targetLink, IProgress<double> progress, string expectedHash = null)
        {
            try
            {
                outputFile.Refresh();

                if (outputFile.Exists) outputFile.Delete();

                // Use the provided extension method
                using (var file = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
                    await _httpClient.DownloadDataAsync(targetLink, file, progress);

                outputFile.Refresh();

                if (!outputFile.Exists)
                {
                    return GenericResult.FromError($"Failed to download {outputFile.Name}");
                }

                if (expectedHash != null && !FileHashCheck(outputFile, expectedHash))
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
    }
}
