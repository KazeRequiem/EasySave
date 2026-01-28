using EasySave.Models;
using EasySave.Strategies;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;

namespace EasySave.Services
{
    public class BackupService
    {
        private readonly string _jsonPath;
        public List<BackupJob> BackupJobs { get; set; }

        public BackupService()
        {
            _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");
            BackupJobs = new List<Models.BackupJob>();
            LoadJobs();
        }

        public void CreateJob(string name, string source, string destination, BackupType backupType)
        {
            int id = BackupJobs.Count + 1;
            if (id > 5)
            {
                throw new InvalidOperationException("[Error] You can't create more than 5 jobs.");
            }

            var newJob = new BackupJob(id, name, source, destination, backupType);
            BackupJobs?.Add(newJob);
            SaveJobs();
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
                SaveJobs();
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
                SaveJobs();
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
            SaveJobs();

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
                SaveJobs();
            }
        }

        private void SaveJobs()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(BackupJobs, options);
            File.WriteAllText(_jsonPath, jsonString);
        }

        private void LoadJobs()
        {
            if (File.Exists(_jsonPath))
            {
                try
                {
                    string jsonString = File.ReadAllText(_jsonPath);
                    BackupJobs = JsonSerializer.Deserialize<List<BackupJob>>(jsonString) ?? new List<BackupJob>();
                }
                catch
                {
                    BackupJobs = new List<BackupJob>();
                }
            }
            else
            {
                BackupJobs = new List<BackupJob>();
            }
        }
    }
}
