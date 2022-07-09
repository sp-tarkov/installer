using Spectre.Console;
using SPT_AKI_Installer.Aki.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SPT_AKI_Installer.Aki.Core.Model
{
    public abstract class LiveTableTask : ILiveTaskTableEntry, IProgressableTask, IDisposable
    {
        /// <summary>
        /// The name that will be displayed in th first column of the table
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Wheather the task reports progress or not
        /// </summary>
        public bool IsIndeterminate;

        /// <summary>
        /// The progress (percent completed) of the task
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// The row index in the table of the task
        /// </summary>
        public int RowIndex { get; set; }

        private bool _continueRenderingProgress = false;
        private bool _continueRenderingIndeterminateProgress = false;

        private int _indeterminateState = 0;

        private string _currentStatus = "running";

        private Table _table { get; set; }

        private LiveDisplayContext _context { get; set; }

        public LiveTableTask(string name, bool isIndeterminate = true)
        {
            TaskName = name;
            IsIndeterminate = isIndeterminate;
        }

        private string GetIndetermminateStatus()
        {
            string status = $"[blue]{_currentStatus.EscapeMarkup()} ";

            if (_indeterminateState > 3) _indeterminateState = 0;

            status += new string('.', _indeterminateState);

            status += "[/]";

            _indeterminateState++;

            return status;
        }

        /// <summary>
        /// Start indeterminate progress spinner
        /// </summary>
        /// <remarks>this doesn't need to be called if you set isIndeterminate in the constructor. You need to set IsIndeterminate to false to stop this background task</remarks>
        public void StartDrawingIndeterminateProgress()
        {
            _continueRenderingProgress = false;
            _continueRenderingIndeterminateProgress = true;

            new Task(new Action(() => { RenderIndeterminateProgress(ref _continueRenderingIndeterminateProgress); })).Start();
        }

        public void StartDrawingProgress()
        {
            Progress = 0;
            _continueRenderingIndeterminateProgress = false;
            _continueRenderingProgress = true;

            new Task(new Action(() => { RenderProgress(ref _continueRenderingProgress); })).Start();
        }

        private void ReRenderEntry(string message)
        {
            _table.RemoveRow(RowIndex);
            _table.InsertRow(RowIndex, TaskName, message);
            _context.Refresh();
        }

        private void RenderIndeterminateProgress(ref bool continueRendering)
        {
            while (continueRendering)
            {
                ReRenderEntry(GetIndetermminateStatus());
                Thread.Sleep(300);
            }
        }

        private void RenderProgress(ref bool continueRendering)
        {
            while (continueRendering)
            {
                string progressBar = new string(' ', 10);

                int progressFill = (int)Math.Floor((double)Progress / 10);

                progressBar = progressBar.Remove(0, progressFill).Insert(0, new string('=', progressFill));

                progressBar = $"[blue][[{progressBar}]][/] {Progress}% {_currentStatus}";

                ReRenderEntry(progressBar);

                Thread.Sleep(300);
            }
        }

        /// <summary>
        /// Set the context and table for this task
        /// </summary>
        /// <param name="context"></param>
        /// <param name="table"></param>
        /// <remarks>This is called by <see cref="LiveTableTaskRunner"/> when it is ran. No need to call it yourself</remarks>
        public void SetContext(LiveDisplayContext context, Table table)
        {
            _context = context;
            _table = table;
        }

        /// <summary>
        /// Set the status text for the task
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="stopRendering">Stop rendering progress updates (progress and indeterminate progress tasks)</param>
        /// <remarks>If you are running an indeterminate task, set render to false. It will render at the next indeterminate update interval</remarks>
        public void SetStatus(string message, bool stopRendering = true)
        {
            _currentStatus = message;

            if (stopRendering)
            {
                _continueRenderingProgress = false;
                _continueRenderingIndeterminateProgress = false;
                ReRenderEntry(message);
            }
        }

        /// <summary>
        /// Run the task async
        /// </summary>
        /// <returns>Returns the result of the task</returns>
        public abstract Task<GenericResult> RunAsync();

        public void Dispose()
        {
            IsIndeterminate = false;
        }
    }
}
