using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public class ProcessChecker : IProcessChecker
    {
        private CancellationTokenSource? cts;
        private bool isCurrentlyRunning = false;

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

        public void StartMonitoring(string processName, Action onProcessStarted, Action onProcessStopped)
        {
            if (string.IsNullOrWhiteSpace(processName)) return;

            StopMonitoring();

            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            isCurrentlyRunning = IsProcessRunning(processName);

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    bool isRunningNow = IsProcessRunning(processName);

                    if (isRunningNow && !isCurrentlyRunning)
                    {
                        isCurrentlyRunning = true;
                        onProcessStarted?.Invoke();
                    }
                    else if (!isRunningNow && isCurrentlyRunning)
                    {
                        isCurrentlyRunning = false;
                        onProcessStopped?.Invoke();
                    }

                    try
                    {
                        await Task.Delay(1000, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, token);
        }

        public void StopMonitoring()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }
    }
}