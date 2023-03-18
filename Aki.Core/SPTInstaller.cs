using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using SPT_AKI_Installer.Aki.Core.Model;
using SPT_AKI_Installer.Aki.Core.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

            var cursorPos = Console.GetCursorPosition();

#if DEBUG
            var path = AnsiConsole.Ask<string>("[purple]DEBUG[/] [blue]::[/] Enter path to install folder: ").Replace("\"", "");

            if (!Directory.Exists(path))
            {
                CloseApp($"Path invalid: {path}");
            }

            _data.TargetInstallPath = path;
#endif
            var continueInstall = AnsiConsole.Confirm($"SPT will install into:\n[blue]{_data.TargetInstallPath}[/]\n\nContinue?", false);

            if (!continueInstall) CloseApp("Please move the installer to the folder you want to install into");


            Console.SetCursorPosition(cursorPos.Left, cursorPos.Top);
            await LiveTableTaskRunner.RunAsync(tasks);
            CloseApp("");
        }

        private static IHost ConfigureHost()
        {
            return Host.CreateDefaultBuilder().ConfigureServices((_, services) =>
            {
                services.AddSingleton<InternalData>();
                services.AddTransient<LiveTableTask, DependencyCheckTask>();
                services.AddTransient<LiveTableTask, InitializationTask>();
                services.AddTransient<LiveTableTask, ReleaseCheckTask>();
                services.AddTransient<LiveTableTask, DownloadTask>();
                services.AddTransient<LiveTableTask, CopyClientTask>();
                services.AddTransient<LiveTableTask, SetupClientTask>();
                services.AddTransient<SPTinstaller>();
            })
            .Build();
        }

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