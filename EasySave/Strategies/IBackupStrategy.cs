using EasySave.Models;
using EasySave.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Strategies
{
    /// <summary>
    /// Defines the contract for backup execution strategies.
    /// 
    /// This interface allows different backup behaviors
    /// (such as full or differential backups) to be implemented
    /// and selected at runtime.
    /// </summary>
    public interface IBackupStrategy
    {
        /// <summary>
        /// Executes a backup operation using the defined strategy.
        /// 
        /// Updates the backup state during execution
        /// and persists state changes through the repository.
        /// </summary>
        double Execute(string sourcePath, string destinationPath, BackupState state, BackupStateRepository stateRepo);
    }
}
