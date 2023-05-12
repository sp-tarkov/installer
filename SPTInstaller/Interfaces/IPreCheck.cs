using System.Threading.Tasks;

namespace SPTInstaller.Interfaces
{
    public interface IPreCheck
    {
        public string Id { get; }
        public string Name { get; }
        public bool IsRequired { get; }

        public bool IsPending { get; set; }
        
        public bool Passed { get; }

        public Task<IResult> RunCheck();
    }
}
