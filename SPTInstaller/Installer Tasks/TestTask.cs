using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System;
using System.Threading.Tasks;

namespace SPTInstaller.Installer_Tasks
{
    internal class TestTask : InstallerTaskBase
    {
        public static TestTask FromRandomName() => new TestTask($"Test Task #{new Random().Next(0, 9999)}");

        public TestTask(string name) : base(name)
        {
        }

        public async override Task<IResult> TaskOperation()
        {
            int total = 4;
            TimeSpan interval = TimeSpan.FromSeconds(1);

            for(int i = 0; i < total; i++)
            {
                var count = i + 1;
                string progressMessage = $"Running Task: {Name}";
                int progress = (int)Math.Floor((double)count / total * 100);

                SetStatus(progressMessage, $"Details: ({count}/{total})", progress);

                await Task.Delay(interval);
            }

            return Result.FromSuccess();
        }
    }
}
