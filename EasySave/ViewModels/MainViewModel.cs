using EasyLog;
using EasySave.Models;
using EasySave.Orchestration;
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
        private readonly BackupStateRepository stateRepository;
        private readonly Orchestrator orchestrator;
        private Settings settings;
        private BackupService backupService;
        private readonly IProcessChecker processChecker;
        public List<BackupJob> backupJobs => backupService.backupJobs;
        public Settings CurrentSettings => backupService.GetSettings();

        public MainViewModel()
        {
            settingsRepository = new BackupSettingsRepository();
            stateRepository = new BackupStateRepository();
            settings = settingsRepository.ReadSettings();
            orchestrator = new Orchestrator(
                settings.maxFileSizeKo,
                settings.priorityExtensions
            );
            this.processChecker = new ProcessChecker();
            backupService = new BackupService(orchestrator);
            StartBusinessSoftwareMonitoring();
        }
        private void StartBusinessSoftwareMonitoring()
        {
            Task.Run(async () =>
            {
                bool isPausedBySoftware = false;

                while (true)
                {
                    string softwareName = CurrentSettings.applicationSoftware;

                    if (!string.IsNullOrWhiteSpace(softwareName))
                    {
                        bool isRunning = processChecker.IsProcessRunning(softwareName);

                        if (isRunning && !isPausedBySoftware)
                        {
                            orchestrator.GlobalPause();
                            isPausedBySoftware = true;
                            Console.WriteLine($"[MONITOR] {softwareName} détecté : Pause forcée.");
                        }
                        else if (!isRunning && isPausedBySoftware)
                        {
                            orchestrator.GlobalResume();
                            isPausedBySoftware = false;
                            Console.WriteLine($"[MONITOR] {softwareName} fermé : Reprise automatique.");
                        }
                    }
                    await Task.Delay(1000);
                }
            });
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
                throw new DirectoryNotFoundException("The source folder doesn't exist :\n" + source);
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
                throw new DirectoryNotFoundException("Error : The source folder could not be found : \n" + source);
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
        public async Task ExecuteJob(int id)
        {
            BackupJob actualJob = backupService.backupJobs.Find(j => j.id == id);
            if (actualJob == null)
            {
                throw new ArgumentException($"The job {id} doesn't exist");
            }
            if (!Directory.Exists(actualJob.sourcePath))
            {
                backupService.LogAction("Execute Job : " + id, actualJob.name, actualJob.sourcePath, actualJob.destinationPath, 0, 0, 0, "[Error] Source folder missing");
                throw new DirectoryNotFoundException($"The source file could not be found :\n{actualJob.sourcePath}");
            }
            try
            {
                await backupService.ExecuteJob(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during copying : {ex.Message}");
                backupService.LogAction("Execute Job : " + id, "None", "None", "None", 0, 0, 0, "[Error] " + ex.Message);
                throw;
            }
        }

        public void StopJob(int id)
        {
            backupService.StopJob(id);
            backupService.LogAction("Stop Job : " + id, "None", "None", "None", 0, 0, 0, "[Success] Work has stopped");
        }

        public void StopAllJobs()
        {
            backupService.StopAllJobs();
            backupService.LogAction("Stop All Job ", "None", "None", "None", 0, 0, 0, "[Success] All work has stopped.");
        }

        public void PauseJob()
        {
            backupService.PauseJob();
            backupService.LogAction("Pause Job ", "None", "None", "None", 0, 0, 0, "[Success] Job paused.");
        }

        public void ResumeJob()
        {
            backupService.ResumeJob();
            backupService.LogAction("Resume Job ", "None", "None", "None", 0, 0, 0, "[Success] Job resumed.");
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
                backupService.LogAction("UpdateLogType : " + logType, "None", "None", "None", 0, 0, 0, "[Success] log = Json");
                backupService.SetLogType(LogFormat.Json);
                Console.WriteLine($"Log Type changed : {LogFormat.Json}");
            }
            else
            {
                backupService.LogAction("UpdateLogType : " + logType, "None", "None", "None", 0, 0, 0, "[Success] log = XML");
                backupService.SetLogType(LogFormat.Xml);
                Console.WriteLine($"Log Type changed : {LogFormat.Xml}");
            }

        }

        public void UpdateCryptKey(string key)
        {
            backupService.SetCryptoKey(key);
            backupService.LogAction("Update Crypt Key : " + key, "None", "None", "None", 0, 0, 0, "[Success] New crypt key");
            Console.WriteLine($"Key changed : {key}");
        }

        public void UpdateCryptPath(string path)
        {
            backupService.SetCryptoPath(path);
            Console.WriteLine($"Path changed : {path}");
            backupService.LogAction("Update Crypt Path : " + path, "None", "None", "None", 0, 0, 0, "[Success] New crypto path");
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

        public void SetMaxFileSize(long maxFileSize)
        {
            if(maxFileSize < 0 )
            {
                backupService.LogAction("Set max file size ", "None", "None", "None", 0, 0, 0, "[Error] Max Size is < 0");
                return;
            }
            backupService.SetMaxFileSize(maxFileSize);
            Console.WriteLine($"File size {maxFileSize} set");
            backupService.LogAction("Set max file size : " + maxFileSize.ToString(), "None", "None", "None", 0, 0, 0, "[Succes] Max Size is update to :"+ maxFileSize.ToString());
        }

        public void AddPriorityExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                Console.WriteLine("Error : Invalid Extension .");
                backupService.LogAction("Add Priority Extension : " + extension, "None", "None", "None", 0, 0, 0, "[Error] New extension is empty");
                return;
            }
            backupService.LogAction("Add Priority Extension : " + extension, "None", "None", "None", 0, 0, 0, "[Success] New extension");
            backupService.AddPriorityExtension(extension);
            Console.WriteLine($"Extension '{extension}' added to the list.");
        }

        public void RemovePriorityExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                backupService.LogAction("Remove Priority Extension : " + extension, "None", "None", "None", 0, 0, 0, "[Error] Extension is empty");
                return;
            }

            backupService.RemovePriorityExtension(extension);
            Console.WriteLine($"Extension '{extension}' deleted.");
            backupService.LogAction("Remove Priority Extension : " + extension, "None", "None", "None", 0, 0, 0, "[Success] Extension is removed");
        }
        public Settings GetCurrentSetting()
        {
            settings = backupService.GetSettings();
            return settings;
        }
        public List<BackupState> GetCurrentStates()
        {
            return stateRepository.ReadStates();
        }

        public double GetGlobalProgress()
        {
            var states = GetCurrentStates();
            if (states == null || states.Count == 0) return 0;
            double totalProgress = 0;
            foreach (var state in states)
            {
                totalProgress += state.progression;
            }
            return totalProgress / states.Count;
        }
    }
}
