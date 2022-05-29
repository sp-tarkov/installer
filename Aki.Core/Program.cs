using Spectre.Console;
using SPT_AKI_Installer.Aki.Helper;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SPT_AKI_Installer.Aki.Core
{
    //TODO:
    // locales, language selection
    // make the installer download relevant version of patcher and aki based on game version if possible

    public static class SPTinstaller
    {
        private static string akiLink;
        private static string patcherLink;
        static void Main()
        {
            string targetPath = Environment.CurrentDirectory;
#if DEBUG
            targetPath = @"D:\install";
#endif

            _ = DownloadFileAsync(targetPath, "https://dev.sp-tarkov.com/api/v1/repos/CWX/Installer_Test/raw/ClientVersions.json", "/ClientVersions.json");
            Console.ReadKey();

            ReadJson(targetPath);
            Console.ReadKey();

            _ = DownloadFileAsync(targetPath, akiLink, "/RELEASE-SPT-2.3.1-17349.zip");
            Console.ReadKey();

            _ = DownloadFileAsync(targetPath, patcherLink, "/Patcher_12.12.15.18103_to_12.12.15.17349.zip");
            Console.ReadKey();

            SpectreHelper.Figlet("SPT-AKI INSTALLER", Color.Yellow);
            PreCheckHelper.GameCheck(out string originalGamePath);

            if (originalGamePath == targetPath)
            {
                CloseApp("Installer is located in EFT's original directory! \n Please move the installer to a seperate folder as per the guide!");
            }

            var checkForExistingFiles = FileHelper.FindFile(targetPath, "EscapeFromTarkov.exe");
            //Console.WriteLine(checkForExistingFiles ?? "null");
            if (checkForExistingFiles != null)
            {
                CloseApp("Installer is located in a Folder that has existing Game Files \n Please make sure the installer is in a fresh folder as per the guide");
            }
            //Console.ReadKey();

            PreCheckHelper.PatcherZipCheck(originalGamePath, targetPath, out string patcherZipPath);
            PreCheckHelper.AkiZipCheck(targetPath, out string akiZipPath);

            if (patcherZipPath == null && PreCheckHelper.PatcherNeededCheck())
            {
                CloseApp("Game Version needs to be patched to match Aki Version \n but Patcher is missing or the wrong version \n Press enter to close the app");
            }

            if (akiZipPath == null)
            {
                CloseApp("Aki's Zip could not be found \n Press enter to close the app");
            }

            if (PreCheckHelper.PatcherNeededCheck() && !PreCheckHelper.PatcherAkiCheck())
            {
                CloseApp("Patcher does not match downgraded version that Aki Requires \n Press enter to close the app");
            }

            LogHelper.Info("Copying game files");

            GameCopy(originalGamePath, targetPath);
            if (PreCheckHelper.PatcherNeededCheck())
            {
                PatcherCopy(targetPath, patcherZipPath);
                PatcherProcess(targetPath);
            }

            AkiInstall(targetPath, akiZipPath);
            DeleteZip(patcherZipPath, akiZipPath);
        }

        static void GameCopy(string originalGamePath, string targetPath)
        {
            FileHelper.CopyDirectory(originalGamePath, targetPath, true);
            LogHelper.Info("Game has been copied, Extracting patcher");
        }

        static void PatcherCopy(string targetPath, string patcherZipPath)
        {
            ZipHelper.Decompress(patcherZipPath, targetPath);
            FileHelper.FindFolder(patcherZipPath, targetPath, out DirectoryInfo dir);
            FileHelper.CopyDirectory(dir.FullName, targetPath, true);

            if (dir.Exists)
            {
                dir.Delete(true);
                dir.Refresh();
                if (dir.Exists)
                {
                    LogHelper.Error("unable to delete patcher folder");
                    LogHelper.Error($"please delete folder called {dir.FullName}");
                }
            }
        }

        static void PatcherProcess(string targetPath)
        {
            LogHelper.Info("patcher has been extracted, starting patcher");
            ProcessHelper patcherProcess = new();
            patcherProcess.StartProcess(Path.Join(targetPath + "/patcher.exe"), targetPath);

            FileHelper.DeleteFiles(Path.Join(targetPath, "/patcher.exe"));
        }

        static void AkiInstall(string targetPath, string akiZipPath)
        {
            ZipHelper.Decompress(akiZipPath, targetPath);
            LogHelper.Info("Aki has been extracted");
        }

        static void DeleteZip(string patcherZipPath, string akiZipPath)
        {
            FileHelper.DeleteFiles(patcherZipPath, false);
            FileHelper.DeleteFiles(akiZipPath, false);

            LogHelper.User("Removed Zips, Press enter to close the installer, you can then delete the installer");
            LogHelper.User("ENJOY SPT-AKI!");
            Console.ReadKey();
            Environment.Exit(0);
        }

        static void CloseApp(string text)
        {
            LogHelper.Warning(text);
            Console.ReadKey();
            Environment.Exit(0);
        }

        // https://dev.sp-tarkov.com/api/v1/repos/CWX/Installer_Test/raw/ClientVersions.json
        // https://dev.sp-tarkov.com/CWX/Installer_Test/src/branch/master/ClientVersions.json

        static async Task DownloadFileAsync(string targetFilePath, string targetLink, string newFileName)
        {
            using (var httpClient = new HttpClient())
            {
                await httpClient.DownloadFile(targetLink, Path.Join(targetFilePath, newFileName));
            }
        }

        static void ReadJson(string targetPath)
        {
            var json = FileHelper.FindFile(targetPath, "ClientVersions.json");
            var text = File.ReadAllText(json);
            dynamic result = JsonConvert.DeserializeObject<object>(text);
            akiLink = result.client18103.AKI;
            patcherLink = result.client18103.PATCHER;
            Console.WriteLine(akiLink);
            Console.WriteLine(patcherLink);
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