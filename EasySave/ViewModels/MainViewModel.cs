using EasyLog;
using EasySave.Models;
using EasySave.Repositories;
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
        private readonly BackupSettingsRepository settingsRepository;
        private Settings settings;
        private BackupService backupService;
        public List<BackupJob> backupJobs => backupService.backupJobs;
        public Settings CurrentSettings => backupService.GetSettings();

        public MainViewModel()
        {
            backupService = new BackupService();
            settingsRepository = new BackupSettingsRepository();
            settings = settingsRepository.ReadSettings();
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
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, 0, "[Error] The name of the job can't be empty");
                return;
            }

            if (!Directory.Exists(source))
            {
                Console.WriteLine("Error : The source folder doesn't exist.");
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, 0, "[Error] The source folder doesn't exist");
                return;
            }

            try
            {
                backupService.CreateJob(name, source, dest, type);
                Console.WriteLine($"Job '{name}' created");
            }
            catch (Exception ex)
            {
                backupService.LogAction("Create Job : " + name, "None", source, dest, 0, 0, 0, "[Error] System Error : " + ex.Message);
                Console.WriteLine($"System Error : {ex.Message}");
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
                Console.WriteLine($"Job {id} deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
                backupService.LogAction("Delete Job : " + id, "None", "None", "None", 0, 0, 0, "[Error] System Error : " + ex.Message);
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
                Console.WriteLine("Error : The source folder could not be found.");
                backupService.LogAction("Modify Job : " + name, "None", source, dest, 0, 0, 0, "[Error] The source folder could not be found");
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
                backupService.LogAction("Modify Job : " + name, "None", source, dest, 0, 0, 0, "[Error] " + ex.Message);
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
            Console.WriteLine($"Job launch {id} in progress...");

            try
            {
                backupService.ExecuteJob(id);
                Console.WriteLine($"Job {id} completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during copying : {ex.Message}");
                backupService.LogAction("Execute Job : " + id, "None", "None", "None", 0, 0, 0, "[Error] " + ex.Message);
                throw;
            }
        }


        public void EditSetting(int id)
        {
            Console.WriteLine($"Setting");

            try
            {
                Console.WriteLine(settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during copying : {ex.Message}");
                backupService.LogAction("Edit Setting : " + id, "None", "None", "None", 0, 0, 0, "[Error] " + ex.Message);
            }
        }

        public void UpdateApplicationSoftware(string softwareName)
        {
            if (string.IsNullOrWhiteSpace(softwareName))
            {
                Console.WriteLine("Error : The name of the software can't be empty.");
                backupService.LogAction("Update Application Software : " + softwareName, "None", "None", "None", 0, 0, 0, "[Error] softwareName error");
                return;
            }

            backupService.SetApplicationSoftware(softwareName);
            Console.WriteLine($"SoftwareName changed : {softwareName}");
            backupService.LogAction("Update Application Software : " + softwareName, "None", "None", "None", 0, 0, 0, "[Success]");
        }

        public void UpdateLogType(string logType)
        {
            if (logType.ToLower() == "json")
            {
                backupService.LogAction("UpdateLogType : " + logType, "None", "None", "None", 0, 0, 0, "[Succes] log = Json");
                backupService.SetLogType(LogFormat.Json);
                Console.WriteLine($"Log Type changed : {LogFormat.Json}");
            }
            else
            {
                backupService.LogAction("UpdateLogType : " + logType, "None", "None", "None", 0, 0, 0, "[Succes] log = XML");
                backupService.SetLogType(LogFormat.Xml);
                Console.WriteLine($"Log Type changed : {LogFormat.Xml}");
            }

        }

        public void UpdateCryptKey(string key)
        {
            backupService.SetCryptoKey(key);
            backupService.LogAction("Update Crypt Key : " + key, "None", "None", "None", 0, 0, 0, "[Succes] New crypt key");
            Console.WriteLine($"Key changed : {key}");
        }

        public void UpdateCryptPath(string path)
        {
            backupService.SetCryptoPath(path);
            Console.WriteLine($"Path changed : {path}");
            backupService.LogAction("Update Crypt Path : " + path, "None", "None", "None", 0, 0, 0, "[Succes] New crypto path");
        }

        public void AddEncryptionExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                Console.WriteLine("Error : Invalid Extension .");
                backupService.LogAction("Add Encryption Extension : " + extension, "None", "None", "None", 0, 0, 0, "[Error] New extension is empty");
                return;
            }
            backupService.LogAction("Add Encryption Extension : " + extension, "None", "None", "None", 0, 0, 0, "[Success] New extension");
            backupService.AddExtensionToEncrypt(extension);
            Console.WriteLine($"Extension '{extension}' added to the list.");
        }

        public void RemoveEncryptionExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                backupService.LogAction("Remove Encryption Extension : " + extension, "None", "None", "None", 0, 0, 0, "[Error] Extension is empty");
                return;
            }

            backupService.RemoveExtensionToEncrypt(extension);
            Console.WriteLine($"Extension '{extension}' deleted.");
            backupService.LogAction("Remove Encryption Extension : " + extension, "None", "None", "None", 0, 0, 0, "[Success] Extension is removed");
        }
        public Settings GetCurrentSetting()
        {
            settings = backupService.GetSettings();
            return settings;
        }

    }
}
