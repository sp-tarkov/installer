using System.Threading.Tasks;

namespace SPTInstaller.Interfaces
{
    public interface IProgressableTask
    {
        public string Id { get; }
        public string Name { get; }

        public bool IsCompleted { get; }

        public bool HasErrors { get; }

        public bool IsRunning { get; }

        public string StatusMessage { get; }

        public int Progress { get; }

        public bool ShowProgress { get; }

        public Task<IResult> RunAsync();
    }
}
