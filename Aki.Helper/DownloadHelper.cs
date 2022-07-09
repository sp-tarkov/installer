using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClientProgress;
using SPT_AKI_Installer.Aki.Core.Model;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class DownloadHelper
    {
        private static HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromHours(1) };

        public static async Task<GenericResult> DownloadFile(FileInfo outputFile, string targetLink, IProgress<double> progress)
        {
            try
            {
                outputFile.Refresh();

                if (outputFile.Exists) outputFile.Delete();

                // Use the provided extension method
                using (var file = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
                    await _httpClient.DownloadDataAsync(targetLink, file, progress);

                outputFile.Refresh();

                if(!outputFile.Exists)
                {
                    return GenericResult.FromError($"Failed to download {outputFile.Name}");
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
