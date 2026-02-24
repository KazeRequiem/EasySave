using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Services
{
    public interface IProcessChecker
    {
        bool IsProcessRunning(string processName);
        void StartMonitoring(string processName, Action onProcessStarted, Action onProcessStopped);
        void StopMonitoring();
    }
}