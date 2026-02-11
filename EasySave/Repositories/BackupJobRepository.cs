using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;

namespace EasySave.Repositories
{
    /// <summary>
    /// Repository responsible for persisting backup job definitions.
    /// 
    /// This class provides a concrete implementation of the
    /// backup job repository interface using JSON file storage.
    /// All read and write operations are handled in a thread-safe manner.
    /// </summary>
    public class BackupJobRepository : IBackupJobRepository
    {
        private readonly string filePath;
        private static readonly object _lock = new object();

        public BackupJobRepository()
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");
        }

        /// <summary>
        /// Reads the list of backup jobs from persistent storage.
        /// 
        /// If the storage file does not exist or cannot be read,
        /// an empty list is returned.
        /// </summary>
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

        /// <summary>
        /// Writes the provided list of backup jobs to persistent storage.
        /// 
        /// Existing data is overwritten and formatted in JSON
        /// for readability.
        /// </summary>
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
