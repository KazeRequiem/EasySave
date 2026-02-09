using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.WPF.Models
{
    /// <summary>
    /// Represents a backup job definition.
    /// 
    /// This model describes the configuration of a backup job,
    /// including source and destination paths, backup type,
    /// and its current execution state.
    /// </summary>
    public class BackupJob
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string sourcePath { get; set; }
        public string destinationPath { get; set; }
        public BackupType type { get; set; }
        public string? state { get; set; }

        /// <summary>
        /// Initializes a new empty backup job.
        /// 
        /// This constructor is mainly used for deserialization.
        /// </summary>
        public BackupJob()
        {
        }

        /// <summary>
        /// Initializes a new backup job with all required parameters.
        /// 
        /// The job state is set to inactive by default.
        /// </summary>
        public BackupJob(int newId, string newName, string newSource, string newDest, BackupType newType)
        {
            id = newId;
            name = newName;
            sourcePath = newSource;
            destinationPath = newDest;
            type = newType;
            state = "Inactive";
        }
    }
}
