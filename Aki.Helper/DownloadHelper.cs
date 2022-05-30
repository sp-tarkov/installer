using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Helper
{
    public static class DownloadHelper
    {
        public static string objToFind;
        public static string patchNeedCheck;
        public static string ogClient;
        public static string targetClient;
        public static string targetAki;
        public static string akiLink;
        public static string patcherLink;

        // https://dev.sp-tarkov.com/api/v1/repos/CWX/Installer_Test/raw/ClientVersions.json
        // https://dev.sp-tarkov.com/CWX/Installer_Test/src/branch/master/ClientVersions.json

        public static async Task DownloadFileAsync(string targetFilePath, string targetLink, string newFileName)
        {
            using (var httpClient = new HttpClient())
            {
                await httpClient.DownloadFile(targetLink, Path.Join(targetFilePath, newFileName));
            }
        }

        public static void ReadJson(string targetPath)
        {
            var json = FileHelper.FindFile(targetPath, "ClientVersions.json");
            var text = File.ReadAllText(json);
            dynamic result = JsonConvert.DeserializeObject<object>(text);

            objToFind = "client" + PreCheckHelper.gameVersion;

            patchNeedCheck = result[objToFind]?.PATCHNEEDED;
            ogClient = result[objToFind]?.OGCLIENT;
            targetClient = result[objToFind]?.TARGETCLIENT;
            targetAki = result[objToFind]?.TARGETAKI;
            akiLink = result[objToFind]?.AKIDOWNLOAD;
            patcherLink = result[objToFind]?.PATCHERDOWNLOAD;

            if (result[objToFind] == null)
            {
                LogHelper.Warning("No records found for client version");
                LogHelper.Warning("this could be because your client is a version before this installer was made or");
                LogHelper.Warning("a new client version has come out, if this is the case, please update to latest or try again later");
                LogHelper.Warning("as we need to update the available list of clients.");
                LogHelper.Warning("Press enter to close the app!");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        public static async Task DownloadFile(this HttpClient client, string address, string fileName)
        {
            using (var response = await client.GetAsync(address))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var file = File.OpenWrite(fileName))
            {
                stream.CopyTo(file);
            }
        }
    }
}
