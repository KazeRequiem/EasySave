using EasySave.WPF.Models;
using EasySave.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace EasySave.WPF.ViewModels
{
    /// <summary>
    /// ViewModel responsible for handling user actions
    /// and connecting the user interface to the backup service.
    /// 
    /// This class validates user inputs and delegates
    /// backup operations to the underlying service layer.
    /// </summary>
    public class MainViewModel
    {
        private BackupService backupService;
        public ObservableCollection<BackupJob> backupJobs { get; set; }

        public MainViewModel()
        {
            backupService = new BackupService();
            backupJobs = new ObservableCollection<BackupJob>(backupService.backupJobs);
        }

        /// <summary>
        /// Creates a new backup job after validating user inputs.
        /// 
        /// Errors are displayed to the user and logged when
        /// invalid parameters or system failures occur.
        /// </summary>
        public void CreateJob(string name, string source, string dest, BackupType type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] The name of the job can't be empty");
                throw new ArgumentException("The name of the job can't be empty.");
            }

            if (!Directory.Exists(source))
            {
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] The source folder doesn't exist");
                throw new DirectoryNotFoundException("The source folder doesn't exist.");
            }

            try
            {
                backupService.CreateJob(name, source, dest, type);
                RefreshList();
            }
            catch (Exception ex)
            {
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] System Error : " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Deletes an existing backup job identified by its ID.
        /// 
        /// Any errors during deletion are displayed and logged.
        /// </summary>
        public void DeleteJob(int id)
        {
            try
            {
                backupService.DeleteJob(id);
                RefreshList();
            }
            catch (Exception ex)
            {
                backupService.LogAction("Delete Job : " + id, "None", "None", "None", 0, 0, "[Error] System Error : " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Modifies an existing backup job after validating input data.
        /// 
        /// If the source directory is invalid or an error occurs,
        /// the operation is aborted and logged.
        /// </summary>
        public void ModifyJob(int id, string name, string source, string dest, BackupType type)
        {
            if (!Directory.Exists(source))
            {
                backupService.LogAction("Modify Job : " + name, "None", source, dest, 0, 0, "[Error] The source folder could not be found");
                throw new DirectoryNotFoundException("The source folder could not be found.");
            }

            try
            {
                backupService.ModifyJob(id, name, source, dest, type);
                RefreshList();
            }
            catch (Exception ex)
            {
                backupService.LogAction("Modify Job : " + name, "None", source, dest, 0, 0, "[Error] " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Executes a backup job identified by its ID.
        /// 
        /// Progress and execution errors are reported to the user
        /// and logged through the service layer.
        /// </summary>
        public void ExecuteJob(int id)
        {
            try
            {
                backupService.ExecuteJob(id);
            }
            catch (Exception ex)
            {
                backupService.LogAction("Execute Job : " + id, "None", "None", "None", 0, 0, "[Error] " + ex.Message);
                throw;
            }
        }

        private void RefreshList()
        {
            backupJobs.Clear();
            foreach (var job in backupService.backupJobs)
            {
                backupJobs.Add(job);
            }
        }
    }
}