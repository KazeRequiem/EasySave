using EasySave.Models;
using System;

namespace EasySave.Strategies
{
    public static class BackupStrategyFactory
    {
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