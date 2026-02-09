using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Services
{
    public interface IProcessChecker
    {
        bool IsProcessRunning(string processName);
    }
}