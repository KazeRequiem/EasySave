using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Strategies
{
    public interface IBackupStrategy
    {
        void Execute(string SourcePath, string DestinationPath);
    }
}
