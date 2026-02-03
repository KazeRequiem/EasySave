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
                LogAction("Create Job : " + name, source, destination, backupType.ToString(), tailleOctetsError, timeError, "[Error] You can't create more than 5 jobs.");
                throw new InvalidOperationException("[Error] You can't create more than 5 jobs.");
            }
            var newJob = new BackupJob(id, name, source, destination, backupType);
            backupJobs.Add(newJob);
            repository.WriteToDisk(backupJobs);
            double timeSuccess = chrono.Elapsed.TotalMilliseconds;
            long tailleOctetsSuccess = GetDirectorySize(source);
            LogAction("Create Job : " + newJob.name, newJob.sourcePath, newJob.destinationPath, newJob.type.ToString(), tailleOctetsSuccess, timeSuccess, "[Success] Job Create.");
        }

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
                LogAction("Create Job : " + name, source, destination, backupType.ToString(), tailleOctetsSuccess, timeSuccess, "[Success].");
            }
            else
            {
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsError = GetDirectorySize(source);
                LogAction("Create Job : " + name, source, destination, backupType.ToString(), tailleOctetsError, timeError, "[Error] No job found with the specified id.");
                throw new ArgumentException("No job found with the specified id.");
            }
        }

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
                LogAction("DeleteJob : " + job.name, job.sourcePath, job.destinationPath, "None", tailleOctetsSuccess, timeSuccess, "[Success] Job Deleted.");
            }
            else
            {
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                LogAction("DeleteJob : " + id, "None", "None", "None", 0, timeError, "[Error] No job found.");
                throw new ArgumentException("No job found with the specified id.");
            }
        }

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
                LogAction("ExecuteJob : " + job.name, job.sourcePath, job.destinationPath, "None", tailleOctetsSuccess, timeSuccess, "[Success] Job Executed.");
            }
            catch (Exception ex)
            {
                state.state = "ERROR";
                stateRepository.UpdateState(state);
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsError = GetDirectorySize(job.destinationPath);
                LogAction("ExecuteJob : " + job.name, job.sourcePath, job.destinationPath, "None", tailleOctetsError, timeError, "[Error] Failed to execute.");
                throw;
            }
            finally
            {
                job.state = "Inactive";
                repository.WriteToDisk(backupJobs);
            }
        }
        public void LogAction(string operation, string name, string source, string destination, long size, double time, string SuccessOrError)
        {
            LogEntry logEntry = new LogEntry
            {
                OperationName = operation,
                NameSave = name,
                SourcePath = source,
                DestinationPath = destination,
                SizeFile = size,
                TimeTransfer = time,
                Success_Error = SuccessOrError
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
