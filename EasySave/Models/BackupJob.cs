using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Models
{
     public class BackupJob
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string sourcePath { get; set; }
        public string destinationPath { get; set; }
        public BackupType type { get; set; }
        public string? state { get; set; }


        public BackupJob()
        {
        }
        public BackupJob(int newId, string newName, string newSource, string newDest, BackupType newType)
        {
            id = newId;
            name = newName;
            sourcePath = newSource;
            destinationPath = newDest;
            type = newType;
            state = "Inactive"; //default state
        }
    }
}