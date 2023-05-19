using Avalonia.Threading;
using ReactiveUI;
using Serilog;
using Splat;
using SPTInstaller.Interfaces;
using System;
using System.Security;
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

        private bool _indeterminateProgress;
        public bool IndeterminateProgress
        {
            get => _indeterminateProgress;
            private set => this.RaiseAndSetIfChanged(ref _indeterminateProgress, value);
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

        public enum ProgressStyle
        {
            Hidden = 0,
            Shown,
            Indeterminate,
        }

        /// <summary>
        /// Update the status details of the task
        /// </summary>
        /// <param name="message">The main message to display. Not updated if null</param>
        /// <param name="details">The details of the task. Not updated if null</param>
        /// <param name="progress">Progress of the task. Overrides progressStyle if a non-null value is supplied</param>
        /// <param name="progressStyle">The style of the progress bar</param>
        public void SetStatus(string? message, string? details, int? progress = null, ProgressStyle? progressStyle = null, bool noLog = false)
        {
            if(message != null && message != StatusMessage)
            {
                if (!noLog && !string.IsNullOrWhiteSpace(message))
                {
                    Log.Information($" <===> {message} <===>");
                }

                StatusMessage = message;
            }

            if(details != null && details != StatusDetails)
            {
                if (!noLog && !string.IsNullOrWhiteSpace(details))
                {
                    Log.Information(details);
                }

                StatusDetails = details;
            }

            if (progressStyle != null)
            {
                switch (progressStyle)
                {
                    case ProgressStyle.Hidden:
                        ShowProgress = false;
                        IndeterminateProgress = false;
                        break;
                    case ProgressStyle.Shown:
                        ShowProgress = true;
                        IndeterminateProgress = false;
                        break;
                    case ProgressStyle.Indeterminate:
                        ShowProgress = true;
                        IndeterminateProgress = true;
                        break;
                }
            }

            if (progress != null)
            {
                ShowProgress = true;
                IndeterminateProgress = false;
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
                HasErrors = true;

                return result;
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
