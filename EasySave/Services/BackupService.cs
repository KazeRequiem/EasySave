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
        public List<BackupJob> BackupJobs { get; set; }

        public BackupService()
        {
            repository = new BackupJobRepository();
            BackupJobs = repository.ReadFromDisk();
        }

        public void CreateJob(string name, string source, string destination, BackupType backupType)
        {
            int id = BackupJobs.Count + 1;
            if (BackupJobs.Count >= 5)
            {
                throw new InvalidOperationException("[Error] You can't create more than 5 jobs.");
            }

            var newJob = new BackupJob(id, name, source, destination, backupType);
            BackupJobs.Add(newJob);
            repository.WriteToDisk(BackupJobs);
        }

        public void ModifyJob(int id, string name, string source, string destination, BackupType backupType)
        {
            BackupJob? job = BackupJobs.Find(j => j.Id == id);
            if (job != null)
            {
                job.Name = name;
                job.SourcePath = source;
                job.DestinationPath = destination;
                job.Type = backupType;
                repository.WriteToDisk(BackupJobs);
            }
            else
            {
                throw new ArgumentException("No job found with the specified id.");
            }
        }

        public void DeleteJob(int id)
        {
            BackupJob? job = BackupJobs.Find(j => j.Id == id);
            if (job != null)
            {
                BackupJobs.Remove(job);
                for (int i = 0; i < BackupJobs.Count; i++)
                {
                    BackupJobs[i].Id = i + 1;
                }
                repository.WriteToDisk(BackupJobs);
            }
            else
            {
                throw new ArgumentException("No job found with the specified id.");
            }
        }   

        public void ExecuteJob(int id)
        {
            BackupJob? job = BackupJobs.Find(j => j.Id == id);
            if (job == null)
            {
                throw new ArgumentException("No job found with the specified id.");
            }

            IBackupStrategy strategy = BackupStrategyFactory.Create(job.Type);

            job.State = "Active";
            repository.WriteToDisk(BackupJobs);

            try
            {
                strategy?.Execute(job.SourcePath, job.DestinationPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] {ex.Message}"); 
            }
            finally
            {
                job.State = "Inactive";
                repository.WriteToDisk(BackupJobs);
            }
        }
    }
}
