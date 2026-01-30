using EasySave.Models;
using EasySave.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Strategies
{
    public interface IBackupStrategy
    {
        void Execute(string sourcePath, string destinationPath, BackupState state, BackupStateRepository stateRepo);
    }
}
