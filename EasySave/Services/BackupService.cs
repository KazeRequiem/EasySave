using EasyLog;
using EasySave.Models;
using EasySave.Repositories;
using EasySave.Strategies;
using EasySave.Orchestration;
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
        private readonly BackupSettingsRepository settingsRepository;
        private readonly IProcessChecker processChecker;
        private readonly Orchestrator orchestrator;
        private readonly IRemoteLogService remoteLogService;

        private Dictionary<int, CancellationTokenSource> runningJobs = new Dictionary<int, CancellationTokenSource>();

        private Settings settings;
        public List<BackupJob> backupJobs { get; set; }

        public BackupService(Orchestrator newOrchestrator)
        {
            repository = new BackupJobRepository();
            stateRepository = new BackupStateRepository();
            backupJobs = repository.ReadFromDisk();
            processChecker = new ProcessChecker();
            settingsRepository = new BackupSettingsRepository();
            settings = settingsRepository.ReadSettings();
            orchestrator = newOrchestrator;
            remoteLogService = new DockerLogService();
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
            var newJob = new BackupJob(id, name, source, destination, backupType);
            backupJobs.Add(newJob);
            repository.WriteToDisk(backupJobs);
            double timeSuccess = chrono.Elapsed.TotalMilliseconds;
            long tailleOctetsSuccess = GetDirectorySize(source);
            LogAction("Create Job : " + newJob.name, newJob.type.ToString(), newJob.sourcePath, newJob.destinationPath, tailleOctetsSuccess, 0, timeSuccess, "[Success] Job Create.");
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
                LogAction("ModifyJob : " + name, backupType.ToString(), source, destination, tailleOctetsSuccess, timeSuccess, 0, "[Success].");
            }
            else
            {
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsError = GetDirectorySize(source);
                LogAction("Modify Job : " + name, backupType.ToString(),source, destination,tailleOctetsError, timeError, 0, "[Error] No job found with the specified id.");
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
                LogAction("DeleteJob : " + job.name, "None", job.sourcePath, job.destinationPath, tailleOctetsSuccess, timeSuccess, 0, "[Success] Job Deleted.");
            }
            else
            {
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                LogAction("DeleteJob : " + id, "None", "None", "None", 0, timeError, 0, "[Error] No job found.");
                throw new ArgumentException("No job found with the specified id.");
            }
        }

        /// <summary>
        /// Executes a backup job identified by its ID.
        /// 
        /// The appropriate backup strategy is selected based on the job type.
        /// Job state is updated during execution and logged accordingly.
        /// </summary>
        public async Task ExecuteJob(int id)
        {
            orchestrator.UndoStop();
            Stopwatch chrono = new Stopwatch();
            chrono.Start();
            BackupJob? job = backupJobs.Find(j => j.id == id);
            if (job == null)
            {
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                LogAction("ExecuteJob : " + id, "None", "None", "None", 0, timeError, 0, "[Error] No job found.");
                throw new ArgumentException("No job found with the specified id.");
            }
            var cts = new CancellationTokenSource();
            if (runningJobs.ContainsKey(id))
            {
                runningJobs.Remove(id);
            }
            runningJobs.Add(id, cts);

            if (!string.IsNullOrWhiteSpace(settings.applicationSoftware) && processChecker.IsProcessRunning(settings.applicationSoftware))
            {
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                LogAction("ExecuteJob : " + id, "None", "None", "None", 0, timeError, 0, "[Error] Other process detected while running.");
                runningJobs.Remove(id);
                throw new ArgumentException("Other process detected while running.");
            }
            IBackupStrategy strategy = BackupStrategyFactory.Create(job.type);

            if (settings.priorityExtensions != null && settings.priorityExtensions.Count > 0 && Directory.Exists(job.sourcePath))
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(job.sourcePath);
                    foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                    {
                        if (orchestrator.IsPriority(file.Extension))
                        {
                            orchestrator.RegisterPriorityFile();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Pre-scan failed for job {id}. Priority might not be optimal. {ex.Message}");
                }
            }

            var state = new BackupState
            {
                name = job.name,
                state = "ACTIVE",
                lastActionTimestamp = DateTime.Now
            };

            job.state = "Active";
            repository.WriteToDisk(backupJobs);

            double encryptionTimeMs = 0;

            try
            {
                encryptionTimeMs = await Task.Run(async () =>
                {
                    return await strategy.Execute(job.sourcePath, job.destinationPath, state, stateRepository, cts.Token, orchestrator);
                }, cts.Token);
                state.state = "END";
                state.nbFilesLeftToDo = 0;
                state.progression = 100;
                state.sourceFilePath = job.sourcePath;
                state.targetFilePath = job.destinationPath;
                stateRepository.UpdateState(state);
                chrono.Stop();
                double timeSuccess = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsSuccess = GetDirectorySize(job.destinationPath);
                LogAction("ExecuteJob : " + job.name, "None", job.sourcePath, job.destinationPath, tailleOctetsSuccess, timeSuccess, encryptionTimeMs, "[Success] Job Executed.");
            }
            catch (Exception ex)
            {
                state.state = "ERROR";
                stateRepository.UpdateState(state);
                chrono.Stop();
                double timeError = chrono.Elapsed.TotalMilliseconds;
                long tailleOctetsError = GetDirectorySize(job.destinationPath);
                LogAction("ExecuteJob : " + job.name, "None", job.sourcePath, job.destinationPath, tailleOctetsError, timeError, -1, "[Error] Failed to execute.");
                throw;
            }
            finally
            {
                job.state = "Inactive";
                repository.WriteToDisk(backupJobs);
            }
        }

        public void StopJob(int id)
        {
            if (runningJobs.TryGetValue(id, out CancellationTokenSource? cts))
            {
                cts.Cancel();
            }
        }

        public void StopAllJobs()
        {
            orchestrator.GlobalStop();
            foreach (var job in backupJobs)
            {
                StopJob(job.id);
            }
            orchestrator.GlobalResume();
        }

        public void PauseJob()
        {
            orchestrator.GlobalPause();
        }

        public void ResumeJob()
        {
            orchestrator.GlobalResume();
        }

        public BackupJob GetJobById(int id)
        {
            for (int i = 0; i < backupJobs.Count; i++)
            {
                BackupJob job = backupJobs[i];
                if (job.id == id)
                {
                    return job;
                }
            }
            throw new Exception("Job not found");
        }
        public void SetApplicationSoftware(string softwareName)
        {
            if (!softwareName.EndsWith(".exe"))
            {
                softwareName += ".exe";
            }

            settings.applicationSoftware = softwareName;
            settingsRepository.WriteSettings(settings);
        }

        public void SetLogType(LogFormat typelog)
        {
            settings.logType = typelog;
            settingsRepository.WriteSettings(settings);
        }

        public void SetCryptoKey(string key)
        {
            settings.cryptoKey = key;
            settingsRepository.WriteSettings(settings);
        }

        public void SetCryptoPath(string path)
        {
            settings.cryptoSoftPath = path;
            settingsRepository.WriteSettings(settings);
        }

        public void AddExtensionToEncrypt(string extension)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            if (!settings.extensionsToEncrypt.Contains(extension))
            {
                settings.extensionsToEncrypt.Add(extension);
                settingsRepository.WriteSettings(settings);
            }
        }

        public void RemoveExtensionToEncrypt(string extension)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            if (settings.extensionsToEncrypt.Contains(extension))
            {
                settings.extensionsToEncrypt.Remove(extension);
                settingsRepository.WriteSettings(settings);
            }
        }

        public void SetMaxFileSize(long sizeKo)
        {
            settings.maxFileSizeKo = sizeKo;
            settingsRepository.WriteSettings(settings);
        }

        public void AddPriorityExtension(string extension)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            if (!settings.priorityExtensions.Contains(extension))
            {
                settings.priorityExtensions.Add(extension);
                settingsRepository.WriteSettings(settings);
            }
        }
        
        public void RemovePriorityExtension(string extension)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            if (settings.priorityExtensions.Contains(extension))
            {
                settings.priorityExtensions.Remove(extension);
                settingsRepository.WriteSettings(settings);
            }
        }

        public void SetLogLocation(LogLocation logLocation)
        {
            settings.logLocation = logLocation;
            settingsRepository.WriteSettings(settings);
        }

        public Settings GetSettings()
        {
            return settings;
        }

        /// <summary>
        /// Logs a backup-related operation into the logging system.
        /// 
        /// This method centralizes logging for both successful
        /// and failed operations.
        /// </summary>
        public void LogAction(string operation, string name, string source, string destination, long size, double time, double crypttime, string successOrError)
        {
            LogFormat formatToUse = settings.logType == LogFormat.Xml ? LogFormat.Xml : LogFormat.Json;
            LogEntry logEntry = new LogEntry
            {
                operationName = operation,
                savetype = name,
                sourcePath = source,
                destinationPath = destination,
                sizeFile = size,
                timeTransfer = time,
                encryptionTimeMs = crypttime,
                success_Error = successOrError,
                formatJsonOrXml = formatToUse
            };
            try
            {
                if (settings.logLocation == LogLocation.local || settings.logLocation == LogLocation.localAndCentralized)
                {
                    Logger.Instance.WriteLog(logEntry);
                }

                if (settings.logLocation == LogLocation.centralized || settings.logLocation == LogLocation.localAndCentralized)
                {
                    _ = remoteLogService.SendLogAsync(logEntry);
                }
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
