using Avalonia.Threading;
using ReactiveUI;
using SPTInstaller.Interfaces;
using System;
using System.Threading.Tasks;

namespace SPTInstaller.Models
{
    public abstract class InstallerTaskBase : ReactiveObject, IProgressableTask
    {
        private string _id;
        public string Id
        {
            get => _id;
            private set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            private set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private bool _isComleted;
        public bool IsCompleted
        {
            get => _isComleted;
            private set => this.RaiseAndSetIfChanged(ref _isComleted, value);
        }

        private bool _hasErrors;
        public bool HasErrors
        {
            get => _hasErrors;
            private set => this.RaiseAndSetIfChanged(ref _hasErrors, value);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            private set => this.RaiseAndSetIfChanged(ref _isRunning, value);
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            private set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private bool _showProgress;
        public bool ShowProgress
        {
            get => _showProgress;
            private set => this.RaiseAndSetIfChanged(ref _showProgress, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        private string _statusDetails;
        public string StatusDetails
        {
            get => _statusDetails;
            private set => this.RaiseAndSetIfChanged(ref _statusDetails, value);
        }

        /// <summary>
        /// Update the status message and optionally a progress bar value
        /// </summary>
        /// <param name="message"></param>
        /// <param name="progress"></param>
        /// <remarks>If message is empty, it isn't updated. If progress is null, the progress bar will be hidden. Details is not touched with this method</remarks>
        public void SetStatus(string message, int? progress = null) => SetStatus(message, "", progress);

        /// <summary>
        /// Update the status message, status details, and optionlly a progress bar value
        /// </summary>
        /// <param name="message"></param>
        /// <param name="progress"></param>
        /// <remarks>If message or details are empty, it isn't updated. If progress is null, the progress bar will be hidden</remarks>
        public void SetStatus(string message, string details, int? progress = null)
        {
            StatusMessage = String.IsNullOrWhiteSpace(message) ? StatusMessage : message;
            StatusDetails = String.IsNullOrWhiteSpace(details) ? StatusDetails : details;
            ShowProgress = progress != null;

            if (progress != null)
            {
                Progress = progress.Value;
            }
        }

        public InstallerTaskBase(string name)
        {
            Name = name;
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// A method for the install controller to call. Do not use this within your task
        /// </summary>
        /// <returns></returns>
        public async Task<IResult> RunAsync()
        {
            IsRunning = true;

            var result = await TaskOperation();

            IsRunning = false;

            if (!result.Succeeded)
            {
                // TODO: handle error state
            }

            IsCompleted = true;

            return result;
        }

        /// <summary>
        /// The task you want to run
        /// </summary>
        /// <returns></returns>
        public abstract Task<IResult> TaskOperation();
    }
}
