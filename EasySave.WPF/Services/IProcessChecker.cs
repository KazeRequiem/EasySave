using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.WPF.Services
{
    public interface IProcessChecker
    {
        bool IsProcessRunning(string processName);
    }
}