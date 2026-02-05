using EasyLog;
using EasySave.Models;
using EasySave.Repositories;
using EasySave.Strategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using System.Diagnostics;
using System.Threading;

namespace EasySave.Services
{
    /// <summary>
    /// Service responsible for managing backup jobs.
    /// 
    /// This class handles:
    /// - creation, modification and deletion of backup jobs,
    /// - execution of backup jobs using the appropriate strategy,
    /// - job state updates and persistence,
    /// - logging of all backup-related operations.
    /// </summary>
    public class BackupService
    {
        private readonly IBackupJobRepository repository;
        private readonly BackupStateRepository stateRepository;
        public List<BackupJob> backupJobs { get; set; }

        public BackupService()
        {
            repository = new BackupJobRepository();
            stateRepository = new BackupStateRepository();
            backupJobs = repository.ReadFromDisk();
        }

        /// <summary>
        /// Creates a new backup job and persists it to disk.
        /// 
        /// A maximum of five backup jobs is allowed.
        /// If this limit is exceeded, an exception is thrown and logged.
        /// </summary>
        public void CreateJob(string name, string source, string destination, BackupType backupType)
        {
            Stopwatch chrono = new Stopwatch();
            chrono.Start();
            int id = backupJobs.Count + 1;
            if (backupJobs.Count >= 5)
            {
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsError = GetDirectorySize(source);
                LogAction("Create Job : " + name, backupType.ToString(), source, destination, tailleOctetsError, timeError, "[Error] You can't create more than 5 jobs.");
                throw new InvalidOperationException("[Error] You can't create more than 5 jobs.");
            }
            var newJob = new BackupJob(id, name, source, destination, backupType);
            backupJobs.Add(newJob);
            repository.WriteToDisk(backupJobs);
            double timeSuccess = chrono.Elapsed.TotalMilliseconds;
            long tailleOctetsSuccess = GetDirectorySize(source);
            LogAction("Create Job : " + newJob.name, newJob.type.ToString(), newJob.sourcePath, newJob.destinationPath, tailleOctetsSuccess, timeSuccess, "[Success] Job Create.");
        }

        /// <summary>
        /// Modifies an existing backup job identified by its ID.
        /// 
        /// If the job does not exist, an exception is thrown and logged.
        /// </summary>
        public void ModifyJob(int id, string name, string source, string destination, BackupType backupType)
        {
            Stopwatch chrono = new Stopwatch();
            chrono.Start();
            BackupJob? job = backupJobs.Find(j => j.id == id);
            if (job != null)
            {
                job.name = name;
                job.sourcePath = source;
                job.destinationPath = destination;
                job.type = backupType;
                repository.WriteToDisk(backupJobs);
                chrono.Stop();
                double timeSuccess = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsSuccess = GetDirectorySize(source);
                LogAction("Create Job : " + name, backupType.ToString(), source, destination, tailleOctetsSuccess, timeSuccess, "[Success].");
            }
            else
            {
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsError = GetDirectorySize(source);
                LogAction("Create Job : " + name, backupType.ToString(),source, destination,tailleOctetsError, timeError, "[Error] No job found with the specified id.");
                throw new ArgumentException("No job found with the specified id.");
            }
        }

        /// <summary>
        /// Deletes a backup job identified by its ID.
        /// 
        /// The remaining jobs are reindexed after deletion.
        /// If the job does not exist, an exception is thrown.
        /// </summary>
        public void DeleteJob(int id)
        {
            Stopwatch chrono = new Stopwatch();
            chrono.Start();
            BackupJob? job = backupJobs.Find(j => j.id == id);
            if (job != null)
            {
                backupJobs.Remove(job);
                for (int i = 0; i < backupJobs.Count; i++)
                {
                    backupJobs[i].id = i + 1;
                }
                repository.WriteToDisk(backupJobs);
                chrono.Stop();
                double timeSuccess = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsSuccess = GetDirectorySize(job.destinationPath);
                LogAction("DeleteJob : " + job.name, "None", job.sourcePath, job.destinationPath, tailleOctetsSuccess, timeSuccess, "[Success] Job Deleted.");
            }
            else
            {
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                LogAction("DeleteJob : " + id, "None", "None", "None", 0, timeError, "[Error] No job found.");
                throw new ArgumentException("No job found with the specified id.");
            }
        }

        /// <summary>
        /// Executes a backup job identified by its ID.
        /// 
        /// The appropriate backup strategy is selected based on the job type.
        /// Job state is updated during execution and logged accordingly.
        /// </summary>
        public void ExecuteJob(int id)
        {
            Stopwatch chrono = new Stopwatch();
            chrono.Start();
            BackupJob? job = backupJobs.Find(j => j.id == id);
            if (job == null)
            {
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                LogAction("ExecuteJob : " + id, "None", "None", "None", 0, timeError, "[Error] No job found.");
                throw new ArgumentException("No job found with the specified id.");
            }

            IBackupStrategy strategy = BackupStrategyFactory.Create(job.type);

            var state = new BackupState
            {
                name = job.name,
                state = "ACTIVE",
                lastActionTimestamp = DateTime.Now
            };

            job.state = "Active";
            repository.WriteToDisk(backupJobs);

            try
            {
                strategy.Execute(job.sourcePath, job.destinationPath, state, stateRepository);
                state.state = "END";
                state.nbFilesLeftToDo = 0;
                state.progression = 100;
                state.sourceFilePath = job.sourcePath;
                state.targetFilePath = job.destinationPath;
                stateRepository.UpdateState(state);
                chrono.Stop();
                double timeSuccess = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsSuccess = GetDirectorySize(job.destinationPath);
                LogAction("ExecuteJob : " + job.name, "None", job.sourcePath, job.destinationPath, tailleOctetsSuccess, timeSuccess, "[Success] Job Executed.");
            }
            catch (Exception ex)
            {
                state.state = "ERROR";
                stateRepository.UpdateState(state);
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsError = GetDirectorySize(job.destinationPath);
                LogAction("ExecuteJob : " + job.name, "None", job.sourcePath, job.destinationPath, tailleOctetsError, timeError, "[Error] Failed to execute.");
                throw;
            }
            finally
            {
                job.state = "Inactive";
                repository.WriteToDisk(backupJobs);
            }
        }

        /// <summary>
        /// Logs a backup-related operation into the logging system.
        /// 
        /// This method centralizes logging for both successful
        /// and failed operations.
        /// </summary>
        public void LogAction(string operation, string name, string source, string destination, long size, double time, string successOrError)
        {
            LogEntry logEntry = new LogEntry
            {
                operationName = operation,
                nameSave = name,
                sourcePath = source,
                destinationPath = destination,
                sizeFile = size,
                timeTransfer = time,
                success_Error = successOrError
            };

            try
            {
                Logger.Instance.WriteLog(logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Daily log : Error during {operation} : {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the total size of a directory in bytes.
        /// 
        /// Returns zero if the directory does not exist.
        /// </summary>
        private long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long size = 0;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (var fileInfo in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                size += fileInfo.Length;
            }
            return size;
        }
    }
}
