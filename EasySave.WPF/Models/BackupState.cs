using System;

namespace EasySave.WPF.Models
{
    /// <summary>
    /// Represents the execution state of a backup job.
    /// 
    /// This model stores real-time information about a backup process,
    /// including progress, current file paths, remaining files,
    /// total size, and execution status.
    /// 
    /// It is used to monitor and persist the state of running
    /// or completed backup operations.
    /// </summary>
    public class BackupState
    {
        public string name { get; set; }
        public string sourceFilePath { get; set; }
        public string targetFilePath { get; set; }
        public string state { get; set; }
        public int totalFilesToCopy { get; set; }
        public long totalFilesSize { get; set; }
        public int nbFilesLeftToDo { get; set; }
        public int progression { get; set; }
        public DateTime lastActionTimestamp { get; set; }

        /// <summary>
        /// Initializes a new backup state with default values.
        /// 
        /// By default, the backup is considered completed and
        /// all counters are set to zero.
        /// </summary>
        public BackupState()
        {
            sourceFilePath = "";
            targetFilePath = "";
            state = "END";
            totalFilesToCopy = 0;
            totalFilesSize = 0;
            nbFilesLeftToDo = 0;
            progression = 0;
            lastActionTimestamp = DateTime.Now;
        }
    }
}
