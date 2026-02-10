using EasyLog;
using EasySave.Models;
using EasySave.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace EasySave.ViewModels
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
        public List<BackupJob> backupJobs => backupService.backupJobs;
        public Settings CurrentSettings => backupService.GetSettings();

        public MainViewModel()
        {
            backupService = new BackupService();
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
                Console.WriteLine("Error : The name of the job can't be empty.");
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] The name of the job can't be empty", LogFormat.Xml);
                return;
            }

            if (!Directory.Exists(source))
            {
                Console.WriteLine("Error : The source folder doesn't exist.");
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] The source folder doesn't exist", LogFormat.Xml);
                return;
            }

            try
            {
                backupService.CreateJob(name, source, dest, type);
                Console.WriteLine($"Job '{name}' created");
            }
            catch (Exception ex)
            {
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] System Error : " + ex.Message, LogFormat.Xml);
                Console.WriteLine($"System Error : {ex.Message}");
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
                Console.WriteLine($"Job {id} deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
                backupService.LogAction("Create Job : " + id, "None", "None", "None", 0, 0, "[Error] System Error : " + ex.Message, LogFormat.Xml);
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
                Console.WriteLine("Error : The source folder could not be found.");
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] The source folder could not be found", LogFormat.Xml);
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
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, "[Error] " + ex.Message, LogFormat.Xml);
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
            Console.WriteLine($"Job launch {id} in progress...");

            try
            {
                backupService.ExecuteJob(id);
                Console.WriteLine($"Job {id} completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during copying : {ex.Message}");
                backupService.LogAction("Create Job : " + id, "None", "None", "None", 0, 0, "[Error] " + ex.Message, LogFormat.Xml);
            }
        }

        public void UpdateApplicationSoftware(string softwareName)
        {
            if (string.IsNullOrWhiteSpace(softwareName))
            {
                Console.WriteLine("Error : The name of the software can't be empty.");
                return;
            }

            backupService.SetApplicationSoftware(softwareName);
            Console.WriteLine($"SoftwareName changed : {softwareName}");
        }

        public void UpdateLogType(BackupLogType logType)
        {
            backupService.SetLogType(logType);
            Console.WriteLine($"Log Type changed : {logType}");
        }

        public void UpdateCryptKey(string key)
        {
            backupService.SetCryptoKey(key);
            Console.WriteLine($"Key changed : {key}");
        }

        public void UpdateCryptPath(string path)
        {
            backupService.SetCryptoPath(path);
            Console.WriteLine($"Path changed : {path}");
        }

        public void AddEncryptionExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                Console.WriteLine("Error : Invalid Extension .");
                return;
            }

            backupService.AddExtensionToEncrypt(extension);
            Console.WriteLine($"Extension '{extension}' added to the list.");
        }

        public void RemoveEncryptionExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) return;

            backupService.RemoveExtensionToEncrypt(extension);
            Console.WriteLine($"Extension '{extension}' deleted.");
        }
    }
}
