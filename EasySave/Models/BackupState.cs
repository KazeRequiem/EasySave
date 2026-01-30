using System;

namespace EasySave.Models
{
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