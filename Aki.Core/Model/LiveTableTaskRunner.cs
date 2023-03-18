using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Core.Model
{
    public class LiveTableTaskRunner
    {
        private static async Task<(bool, LiveTableTask task)> RunAllTasksAsync(List<LiveTableTask> tasks, LiveDisplayContext context, Table table)
        {
            foreach (var task in tasks)
            {
                if (task.IsIndeterminate)
                {
                    task.StartDrawingIndeterminateProgress();
                }
                else
                {
                    task.StartDrawingProgress();
                }

                var result = await task.RunAsync();

                // critical: error - stop installer
                if (!result.Succeeded && !result.NonCritical)
                {
                    task.SetStatus($"[red]{result.Message.EscapeMarkup()}[/]");
                    return (false, task);
                }

                // non-critical: warning - continue
                if (!result.Succeeded && result.NonCritical)
                {
                    task.SetStatus($"[yellow]{result.Message.EscapeMarkup()}[/]");
                    continue;
                }

                //suceeded: continue
                task.SetStatus($"[green]{(result.Message == "" ? "Complete" : $"{result.Message.EscapeMarkup()}")}[/]");
            }

            return (true, null);
        }
        public static async Task RunAsync(List<LiveTableTask> tasks)
        {
            int halfBufferWidth = Console.BufferWidth / 2;

            Table table = new Table().Alignment(Justify.Center).Border(TableBorder.Rounded).BorderColor(Color.Grey).AddColumn("Task", (column) =>
            {
                column.Width(halfBufferWidth);
            })
            .AddColumn("Status", (column) =>
            {
                column.Width(halfBufferWidth);
            });

            await AnsiConsole.Live(table).StartAsync(async context =>
            {
                foreach (var task in tasks)
                {
                    table.AddRow(task.TaskName, "[purple]Pending[/]");
                    task.RowIndex = table.Rows.Count() - 1;
                    task.SetContext(context, table);

                    await Task.Delay(50);
                    context.Refresh();
                }

                var result = await RunAllTasksAsync(tasks, context, table);

                // if a task failed and returned early, set any remaining task status to cancelled
                if (!result.Item1)
                {
                    var cancelledTasks = tasks.Take(new Range(tasks.IndexOf(result.Item2) + 1, tasks.Count));

                    foreach (var task in cancelledTasks)
                    {
                        task.SetStatus("[grey]Cancelled[/]");
                    }

                }
            });
        }
    }
}
