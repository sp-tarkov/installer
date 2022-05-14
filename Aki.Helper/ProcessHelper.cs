using System.Diagnostics;

namespace SPT_AKI_Installer.Aki.Helper
{
    public class ProcessHelper
    {
        private Process _process;

        /// <summary>
        /// Starts process with path and file name including include .exe,
        /// sets working directory too
        /// </summary>
        public void StartProcess(string exeDir, string workingDir)
        {
            _process = new Process();
            _process.StartInfo.FileName = exeDir;
            _process.StartInfo.WorkingDirectory = workingDir;
            _process.Start();
        }

        /// <summary>
        /// Kills the Process
        /// </summary>
        public void EndProcess()
        {
            _process.Kill();
        }
    }
}
