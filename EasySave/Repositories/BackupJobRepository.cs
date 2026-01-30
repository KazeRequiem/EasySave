using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EasySave.Repositories
{
    public class BackupJobRepository : IBackupJobRepository
    {
        private readonly string filePath;
        private static readonly object _lock = new object();
        public BackupJobRepository()
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");
        }

        public List<BackupJob> ReadFromDisk()
        {
            lock (_lock)
            {
                if (!File.Exists(filePath)) return new List<BackupJob>();
                try
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
                }
                catch { return new List<BackupJob>(); }
            }
        }

        public void WriteToDisk(List<BackupJob> jobs)
        {
            lock (_lock)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                 string json = JsonSerializer.Serialize(jobs, options);
                File.WriteAllText(filePath, json);
            }
        }
    }

}
