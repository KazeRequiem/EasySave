using EasySave.Models;
using EasySave.Strategies;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;
using EasySave.Repositories;

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
            int id = backupJobs.Count + 1;
            if (backupJobs.Count >= 5)
            {
                throw new InvalidOperationException("[Error] You can't create more than 5 jobs.");
            }

            var newJob = new BackupJob(id, name, source, destination, backupType);
            backupJobs.Add(newJob);
            repository.WriteToDisk(backupJobs);
        }

        public void ModifyJob(int id, string name, string source, string destination, BackupType backupType)
        {
            BackupJob? job = backupJobs.Find(j => j.id == id);
            if (job != null)
            {
                job.name = name;
                job.sourcePath = source;
                job.destinationPath = destination;
                job.type = backupType;
                repository.WriteToDisk(backupJobs);
            }
            else
            {
                throw new ArgumentException("No job found with the specified id.");
            }
        }

        public void DeleteJob(int id)
        {
            BackupJob? job = backupJobs.Find(j => j.id == id);
            if (job != null)
            {
                backupJobs.Remove(job);
                for (int i = 0; i < backupJobs.Count; i++)
                {
                    backupJobs[i].id = i + 1;
                }
                repository.WriteToDisk(backupJobs);
            }
            else
            {
                throw new ArgumentException("No job found with the specified id.");
            }
        }   

        public void ExecuteJob(int id)
        {
            BackupJob? job = backupJobs.Find(j => j.id == id);
            if (job == null)
            {
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
            }
            catch (Exception ex)
            {
                state.state = "ERROR";
                stateRepository.UpdateState(state);
                throw;
            }
            finally
            {
                job.state = "Inactive";
                repository.WriteToDisk(backupJobs);
            }
        }
    }
}
