using EasySave.WPF.Models;
using System;

namespace EasySave.WPF.Strategies
{
    /// <summary>
    /// Factory class responsible for creating backup strategy instances.
    /// 
    /// This class centralizes the instantiation logic of backup strategies
    /// and returns the appropriate implementation based on the backup type.
    /// </summary>
    public static class BackupStrategyFactory
    {
        /// <summary>
        /// Creates and returns a backup strategy corresponding to the given backup type.
        /// 
        /// Uses the Factory design pattern to decouple strategy selection
        /// from the backup execution logic.
        /// 
        /// Throws an exception if the backup type is not supported.
        /// </summary>
        public static IBackupStrategy Create(BackupType type)
        {
            switch (type)
            {
                case BackupType.Full:
                    return new FullBackupStrategy();

                case BackupType.Differential:
                    return new DifferentialBackupStrategy();

                default:
                    throw new ArgumentException("Unknown backup type");
            }
        }
    }
}
