using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Spectre.Console;
using System.IO;
using SPT_AKI_Installer.Aki.Core.Model;
using SPT_AKI_Installer.Aki.Core.Tasks;
using System.Collections.Generic;

namespace SPT_AKI_Installer.Aki.Core
{
    //TODO:
    // locales, language selection

    public class SPTinstaller
    {
        InternalData _data;
        static void Main()
        {
            var host = ConfigureHost();

            var tasks = host.Services.GetServices<LiveTableTask>();

            var taskList = new List<LiveTableTask>(tasks);

            AnsiConsole.Write(new FigletText("SPT-AKI INSTALLER").Centered().Color(Color.Yellow));

            host.Services.GetRequiredService<SPTinstaller>()
                         .RunTasksAsync(taskList)
                         .GetAwaiter()
                         .GetResult();

            //            GameCopy(originalGamePath, targetPath);

            //            if (DownloadHelper.patchNeedCheck)
            //            {
            //                PatcherCopy(targetPath, patcherZipPath);
            //                PatcherProcess(targetPath);
            //            }

            //            AkiInstall(targetPath, akiZipPath);
            //            DeleteZip(patcherZipPath, akiZipPath);
            //        }

            //        static void GameCopy(string originalGamePath, string targetPath)
            //        {
            //            LogHelper.Info("Copying game files");
            //            FileHelper.CopyDirectory(originalGamePath, targetPath, true);
            //        }

            //        static void PatcherCopy(string targetPath, string patcherZipPath)
            //        {
            //            LogHelper.Info("Extracting patcher");
            //            ZipHelper.ZipDecompress(patcherZipPath, targetPath);
            //            FileHelper.FindFolder(patcherZipPath, targetPath, out DirectoryInfo dir);
            //            FileHelper.CopyDirectory(dir.FullName, targetPath, true);

            //            if (dir.Exists)
            //            {
            //                dir.Delete(true);
            //                dir.Refresh();
            //                if (dir.Exists)
            //                {
            //                    LogHelper.Error("unable to delete patcher folder");
            //                    LogHelper.Error($"please delete folder called {dir.FullName}");
            //                }
            //            }
        }

        public SPTinstaller(
            InternalData data
            )
        {
            _data = data;
        }

        public async Task RunTasksAsync(List<LiveTableTask> tasks)
        {
            _data.TargetInstallPath = Environment.CurrentDirectory;

#if DEBUG
            var path = AnsiConsole.Ask<string>("[purple]DEBUG[/] [blue]::[/] Enter path to install folder: ").Replace("\"", "");

            if (!Directory.Exists(path))
            {
                CloseApp($"Path invalid: {path}");
            }

            _data.TargetInstallPath = path;
#endif

            await LiveTableTaskRunner.RunAsync(tasks);
        }

        private static IHost ConfigureHost()
        {
            return Host.CreateDefaultBuilder().ConfigureServices((_, services) =>
            {
                services.AddSingleton<InternalData>();
                services.AddTransient<LiveTableTask, InitializationTask>();
                services.AddTransient<LiveTableTask, ReleaseCheckTask>();
                services.AddTransient<LiveTableTask, DownloadTask>();
                services.AddTransient<LiveTableTask, CopyClientTask>();
                services.AddTransient<SPTinstaller>();
            })
            .Build();
        }

        //static void PatcherProcess(string targetPath)
        //{
        //    LogHelper.Info("Starting patcher");
        //    ProcessHelper patcherProcess = new();
        //    patcherProcess.StartProcess(Path.Join(targetPath + "/patcher.exe"), targetPath);

        //    FileHelper.DeleteFiles(Path.Join(targetPath, "/patcher.exe"));
        //}

        //static void AkiInstall(string targetPath, string akiZipPath)
        //{
        //    ZipHelper.ZipDecompress(akiZipPath, targetPath);
        //    LogHelper.Info("Aki has been extracted");
        //}

        //static void DeleteZip(string patcherZipPath, string akiZipPath)
        //{
        //    FileHelper.DeleteFiles(patcherZipPath, false);
        //    FileHelper.DeleteFiles(akiZipPath, false);

        //    LogHelper.User("Removed Zips, Press enter to close the installer, you can then delete the installer");
        //    LogHelper.User("ENJOY SPT-AKI!");
        //    Console.ReadKey();
        //    Environment.Exit(0);
        //}

        static void CloseApp(string text)
        {
            AnsiConsole.MarkupLine($"[yellow]{text.EscapeMarkup()}[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("Press [blue]Enter[/] to close ...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}