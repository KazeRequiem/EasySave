using EasySave.Models;
using EasySave.Services;
using System;
using System.Collections.Generic;
using System.Text;

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
                return;
            }

            if (!Directory.Exists(source))
            {
                return;
            }

            try
            {
                backupService.CreateJob(name, source, dest, type);
            }
            catch (Exception ex)
            {
            }
        }

        public void DeleteJob(int id)
        {
            try
            {
                backupService.DeleteJob(id);
            }
            catch (Exception ex)
            {
            }
        }

        public void ModifyJob(int id, string name, string source, string dest, BackupType type)
        {
            if (!Directory.Exists(source))
            {
                return;
            }

            try
            {
                backupService.ModifyJob(id, name, source, dest, type);
            }
            catch (Exception ex)
            {
            }
        }
        public void ExecuteJob(int id)
        {
            try
            {
                backupService.ExecuteJob(id);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
