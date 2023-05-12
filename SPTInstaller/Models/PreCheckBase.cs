using ReactiveUI;
using SPTInstaller.Interfaces;
using System;
using System.Threading.Tasks;

namespace SPTInstaller.Models
{
    public abstract class PreCheckBase : ReactiveObject, IPreCheck
    {
        private string _id;
        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private bool _required;
        public bool IsRequired
        {
            get => _required;
            set => this.RaiseAndSetIfChanged(ref _required, value);
        }

        private bool _passed;
        public bool Passed
        {
            get => _passed;
            set => this.RaiseAndSetIfChanged(ref _passed, value);
        }

        private bool _isPending;
        public bool IsPending
        {
            get => _isPending;
            set => this.RaiseAndSetIfChanged(ref _isPending, value);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => this.RaiseAndSetIfChanged(ref _isRunning, value);
        }

        /// <summary>
        /// Base class for pre-checks to run before installation
        /// </summary>
        /// <param name="name">The display name of the pre-check</param>
        /// <param name="required">If installation should stop on failing this pre-check</param>
        public PreCheckBase(string name, bool required)
        {
            Name = name;
            IsRequired = required;
            Id = Guid.NewGuid().ToString();
        }

        public async Task<IResult> RunCheck()
        {
            IsRunning = true;
            Passed = await CheckOperation();
            IsRunning = false;
            IsPending = false;

            return Passed ? Result.FromSuccess() : Result.FromError($"PreCheck Failed: {Name}");
        }

        public abstract Task<bool> CheckOperation();
    }
}
