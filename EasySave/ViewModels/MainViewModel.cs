using EasySave.Models;
using EasySave.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace EasySave.ViewModels
{
    public class MainViewModel
    {
        private BackupService backupService;
        public List<BackupJob> backupJobs => backupService.backupJobs;

        public MainViewModel()
        {
            backupService = new BackupService();
        }

        public void CreateJob(string name, string source, string dest, BackupType type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Error : The name of the job can't be empty.");
                backupService.LogAction("Create Job : "+name, "None", source, dest, 0, 0, "[Error] The name of the job can't be empty");
                return;
            }

            if (!Directory.Exists(source))
            {
                Console.WriteLine("Error : The source folder doesn't exist.");
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] The source folder doesn't exist");

                return;
            }

            try
            {
                backupService.CreateJob(name, source, dest, type);
                Console.WriteLine($"Job '{name}' created");
            }
            catch (Exception ex)
            {
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] System Error : "+ex.Message);
                Console.WriteLine($"System Error : {ex.Message}");
            }
        }

        public void DeleteJob(int id)
        {
            try
            {
                backupService.DeleteJob(id);
                Console.WriteLine($"Job {id} deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
                backupService.LogAction("Create Job : " + id, "None", "None", "None", 0, 0, "[Error] System Error : " + ex.Message);
            }
        }

        public void ModifyJob(int id, string name, string source, string dest, BackupType type)
        {
            if (!Directory.Exists(source))
            {
                Console.WriteLine("Error : The source folder could not be found.");
                backupService.LogAction("Create Job : " + name,"None" , source, dest, 0, 0, "[Error] The source folder could not be found");
                return;
            }

            try
            {
                backupService.ModifyJob(id, name, source, dest, type);
                Console.WriteLine($"Job {id} modified");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] "+ ex.Message);
            }
        }
        public void ExecuteJob(int id)
        {
            Console.WriteLine($"Job launch {id} in progress...");

            try
            {
                backupService.ExecuteJob(id);
                Console.WriteLine($"Job {id} completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during copying : {ex.Message}");
                backupService.LogAction("Create Job : " + id, "None", "None", "None", 0, 0, "[Error] " + ex.Message);
            }
        }
    }
}
