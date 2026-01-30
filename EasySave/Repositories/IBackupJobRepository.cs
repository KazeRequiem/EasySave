using EasySave.Models;

namespace EasySave.Repositories
{
    public interface IBackupJobRepository
    {
        List<BackupJob> ReadFromDisk();
        void WriteToDisk(List<BackupJob> jobs);
    }
}
