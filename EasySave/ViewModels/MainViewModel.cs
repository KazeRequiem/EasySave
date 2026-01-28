using EasySave.Models;
using EasySave.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.ViewModels
{
    public class MainViewModel
    {
        private BackupService _backupService;
        public List<BackupJob> BackupJobs => _backupService.BackupJobs;

        public MainViewModel()
        {
            _backupService = new BackupService();
        }

        public void CreateJob(string name, string source, string dest, BackupType type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("❌ Error : The name of the job can't be empty.");
                return;
            }

            if (!Directory.Exists(source))
            {
                Console.WriteLine("❌ Error : The source folder doesn't exist.");
                return;
            }

            try
            {
                _backupService.CreateJob(name, source, dest, type);
                Console.WriteLine($"✅ Job '{name}' created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ System Error : {ex.Message}");
            }
        }

        public void DeleteJob(int id)
        {
            try
            {
                _backupService.DeleteJob(id);
                Console.WriteLine($"✅ Job {id} deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error : {ex.Message}");
            }
        }

        public void ModifyJob(int id, string name, string source, string dest, BackupType type)
        {
            if (!Directory.Exists(source))
            {
                Console.WriteLine("❌ Error : The source folder could not be found.");
                return;
            }

            try
            {
                _backupService.ModifyJob(id, name, source, dest, type);
                Console.WriteLine($"✅ Job {id} modified");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error : {ex.Message}");
            }
        }
        public void ExecuteJob(int id)
        {
            Console.WriteLine($"Job launch {id} in progress...");

            try
            {
                _backupService.ExecuteJob(id);
                Console.WriteLine($"✅ Job {id} completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during copying : {ex.Message}");
            }
        }
    }
}
