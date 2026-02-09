using EasySave.WPF.Models;

namespace EasySave.WPF.Repositories
{
    /// <summary>
    /// Defines the contract for backup job persistence.
    /// 
    /// This interface abstracts the storage mechanism used
    /// to save and load backup jobs from disk.
    /// </summary>
    public interface IBackupJobRepository
    {
        /// <summary>
        /// Reads and returns the list of backup jobs from persistent storage.
        /// </summary>
        List<BackupJob> ReadFromDisk();

        /// <summary>
        /// Writes the provided list of backup jobs to persistent storage.
        /// </summary>
        void WriteToDisk(List<BackupJob> jobs);
    }
}
