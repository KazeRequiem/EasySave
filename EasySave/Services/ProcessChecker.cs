using System.Diagnostics;
using System.Linq;

namespace EasySave.Services
{
    public class ProcessChecker : IProcessChecker
    {
        /// <summary>
        /// Check if a process is running.
        /// Return true or false if the process is running or not.
        /// </summary>
        public bool IsProcessRunning(string processName)
        {
            if (string.IsNullOrEmpty(processName)) return false;
            string nameCleaned = processName.Replace(".exe", "");
            var processes = Process.GetProcessesByName(nameCleaned);
            return processes.Any();
        }
    }
}