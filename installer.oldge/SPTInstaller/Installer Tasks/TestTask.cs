using SPTInstaller.Interfaces;
using SPTInstaller.Models;
using System.Threading.Tasks;

namespace SPTInstaller.Installer_Tasks;

internal class TestTask : InstallerTaskBase
{
    public static TestTask FromRandomName() => new TestTask($"Test Task #{new Random().Next(0, 9999)}");
    
    public TestTask(string name) : base(name)
    {
    }
    
    public async override Task<IResult> TaskOperation()
    {
        var total = 4;
        var interval = TimeSpan.FromSeconds(1);
        
        for (var i = 0; i < total; i++)
        {
            var count = i + 1;
            var progressMessage = $"Running Task: {Name}";
            var progress = (int)Math.Floor((double)count / total * 100);
            
            SetStatus(progressMessage, $"Details: ({count}/{total})", progress);
            
            await Task.Delay(interval);
        }
        
        return Result.FromSuccess();
    }
}