using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Models
{
     public class BackupJob
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? SourcePath { get; set; }
        public string? DestinationPath { get; set; }
        public BackupType? Type { get; set; }
        public string? State { get; set; }


        public BackupJob()
        {
        }
        public BackupJob(int id, string name, string source, string dest, BackupType type)
        {
            Id = id;
            Name = name;
            SourcePath = source;
            DestinationPath = dest;
            Type = type;
            State = "Inactive"; //default state
        }
    }
}