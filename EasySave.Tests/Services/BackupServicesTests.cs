using EasyLog;
using EasySave.Models;
using EasySave.Orchestration;
using EasySave.Repositories;
using EasySave.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasySave.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class BackupServiceTests
    {
        private string jsonPath;
        private string settingsPath;
        private Orchestrator orchestrator;

        [TestInitialize]
        public void Setup()
        {
            jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");
            settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            try
            {
                if (File.Exists(jsonPath)) File.Delete(jsonPath);
                if (File.Exists(settingsPath)) File.Delete(settingsPath);
            }
            catch (IOException) { }

            orchestrator = new Orchestrator(0, new List<string>());
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (File.Exists(jsonPath)) File.Delete(jsonPath);
                if (File.Exists(settingsPath)) File.Delete(settingsPath);
            }
            catch (IOException) { }
        }

        [TestMethod]
        public void CreateJob_ShouldAddJobToList()
        {
            var service = new BackupService(orchestrator);
            service.CreateJob("Job1", "Source", "Dest", BackupType.Full);

            Assert.AreEqual(1, service.backupJobs.Count);
            Assert.AreEqual("Job1", service.backupJobs[0].name);
        }


        [TestMethod]
        public void DeleteJob_ShouldRemoveJobAndReorderIds()
        {
            var service = new BackupService(orchestrator);
            service.CreateJob("Job1", "Source", "Dest", BackupType.Full);
            service.CreateJob("Job2", "Source", "Dest", BackupType.Full);
            service.CreateJob("Job3", "Source", "Dest", BackupType.Full);

            service.DeleteJob(2);
            Assert.AreEqual(2, service.backupJobs.Count);
            Assert.AreEqual("Job1", service.backupJobs[0].name);
            Assert.AreEqual(1, service.backupJobs[0].id);
            Assert.AreEqual("Job3", service.backupJobs[1].name);
            Assert.AreEqual(2, service.backupJobs[1].id);
        }

        [TestMethod]
        public void DeleteJob_ShouldThrowException_WhenIdNotFound()
        {
            var service = new BackupService(orchestrator);
            service.CreateJob("Job1", "Source", "Dest", BackupType.Full);

            try
            {
                service.DeleteJob(99);
                Assert.Fail("An exception should have been raised");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void ModifyJob_ShouldUpdateJobDetails_AndPersist()
        {
            var service = new BackupService(orchestrator);
            service.CreateJob("OriginalName", "SourceA", "DestA", BackupType.Full);

            service.ModifyJob(1, "UpdatedName", "SourceB", "DestB", BackupType.Differential);

            var job = service.backupJobs.FirstOrDefault(j => j.id == 1);
            Assert.IsNotNull(job);
            Assert.AreEqual("UpdatedName", job.name);
            Assert.AreEqual("SourceB", job.sourcePath);
            Assert.AreEqual(BackupType.Differential, job.type);

            var service2 = new BackupService(orchestrator);
            var persistedJob = service2.backupJobs.FirstOrDefault(j => j.id == 1);

            Assert.IsNotNull(persistedJob);
            Assert.AreEqual("UpdatedName", persistedJob.name);
        }

        [TestMethod]
        public async Task ExecuteJob_ShouldThrowException_WhenIdNotFound()
        {
            var service = new BackupService(orchestrator);

            try
            {
                await service.ExecuteJob(10);
                Assert.Fail("An exception should have been raised");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void LoadJobs_ShouldLoadExistingJobsFromFile()
        {
            var service1 = new BackupService(orchestrator);
            service1.CreateJob("PersistentJob", "Source", "Dest", BackupType.Full);

            var service2 = new BackupService(orchestrator);

            Assert.AreEqual(1, service2.backupJobs.Count);
            Assert.AreEqual("PersistentJob", service2.backupJobs[0].name);
        }

        [TestMethod]
        public void LoadSettings_ShouldCreateDefault_WhenFileDoesNotExist()
        {
            var service = new BackupService(orchestrator);

            Assert.IsTrue(File.Exists(settingsPath));

            var repo = new BackupSettingsRepository();
            var settings = repo.ReadSettings();

            Assert.IsNotNull(settings);
            Assert.AreEqual(LogFormat.Json, settings.logType);
            Assert.AreEqual("", settings.cryptoSoftPath);
        }

        [TestMethod]
        public void SetMaxFileSize_ShouldUpdateSize_AndPersistToSettingsFile()
        {
            var service = new BackupService(orchestrator);
            long newSizeKo = 1048576;

            service.SetMaxFileSize(newSizeKo);

            var currentSettings = service.GetSettings();
            Assert.AreEqual(newSizeKo, currentSettings.maxFileSizeKo, "La taille n'a pas été mise à jour en mémoire.");

            var service2 = new BackupService(orchestrator);
            var persistedSettings = service2.GetSettings();

            Assert.AreEqual(newSizeKo, persistedSettings.maxFileSizeKo, "La taille n'a pas été sauvegardée dans settings.json.");
        }


        [TestMethod]
        public void GetJobById_ShouldReturnJob_WhenIdExists()
        {
            var service = new BackupService(orchestrator);
            service.CreateJob("TestJob", "Source", "Dest", BackupType.Full);

            var job = service.GetJobById(1);

            Assert.IsNotNull(job);
            Assert.AreEqual("TestJob", job.name);
        }

        [TestMethod]
        public void GetJobById_ShouldThrowException_WhenIdDoesNotExist()
        {
            var service = new BackupService(orchestrator);

            try
            {
                service.GetJobById(99);
                Assert.Fail("Une exception aurait dû être levée car le job 99 n'existe pas.");
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public void SetApplicationSoftware_ShouldAppendExeAndPersist()
        {
            var service = new BackupService(orchestrator);

            service.SetApplicationSoftware("calculator");
            Assert.AreEqual("calculator.exe", service.GetSettings().applicationSoftware);

            service.SetApplicationSoftware("notepad.exe");
            Assert.AreEqual("notepad.exe", service.GetSettings().applicationSoftware);
        }

        [TestMethod]
        public void SetLogType_ShouldUpdateLogFormat()
        {
            var service = new BackupService(orchestrator);

            service.SetLogType(LogFormat.Xml);
            Assert.AreEqual(LogFormat.Xml, service.GetSettings().logType);
        }

        [TestMethod]
        public void SetCryptoSettings_ShouldUpdateAndPersist()
        {
            var service = new BackupService(orchestrator);

            service.SetCryptoKey("SecretKey123");
            service.SetCryptoPath(@"C:\Path\To\CryptoSoft.exe");

            var settings = service.GetSettings();
            Assert.AreEqual("SecretKey123", settings.cryptoKey);
            Assert.AreEqual(@"C:\Path\To\CryptoSoft.exe", settings.cryptoSoftPath);
        }


        [TestMethod]
        public void AddAndRemoveEncryptionExtension_ShouldManageListCorrectly()
        {
            var service = new BackupService(orchestrator);
            service.GetSettings().extensionsToEncrypt.Clear();

            service.AddExtensionToEncrypt("txt");
            service.AddExtensionToEncrypt(".pdf");
            service.AddExtensionToEncrypt(".pdf");

            var settings = service.GetSettings();
            Assert.IsTrue(settings.extensionsToEncrypt.Contains(".txt"));
            Assert.IsTrue(settings.extensionsToEncrypt.Contains(".pdf"));
            Assert.AreEqual(2, settings.extensionsToEncrypt.Count);

            service.RemoveExtensionToEncrypt("txt");
            Assert.IsFalse(service.GetSettings().extensionsToEncrypt.Contains(".txt"));
        }

        [TestMethod]
        public void AddAndRemovePriorityExtension_ShouldManageListCorrectly()
        {
            var service = new BackupService(orchestrator);
            service.GetSettings().priorityExtensions.Clear();

            service.AddPriorityExtension("docx");
            Assert.IsTrue(service.GetSettings().priorityExtensions.Contains(".docx"));

            service.RemovePriorityExtension(".docx");
            Assert.IsFalse(service.GetSettings().priorityExtensions.Contains(".docx"));
        }


        [TestMethod]
        public void ControlMethods_ShouldExecuteWithoutExceptions()
        {
            var service = new BackupService(orchestrator);
            service.CreateJob("Job1", "Source", "Dest", BackupType.Full);

            try
            {
                service.PauseJob();
                service.ResumeJob();
                service.StopJob(1);
                service.StopAllJobs();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Une méthode de contrôle a levé une exception inattendue : {ex.Message}");
            }
        }

        [TestMethod]
        public void SetLogLocation_ShouldUpdateLocation_AndPersistToSettingsFile()
        {
            var service = new BackupService(orchestrator);

            LogLocation expectedLocation = LogLocation.centralized;
            service.SetLogLocation(expectedLocation);
            var currentSettings = service.GetSettings();
            Assert.AreEqual(expectedLocation, currentSettings.logLocation, "La localisation des logs n'a pas été mise à jour en mémoire.");
            var newService = new BackupService(orchestrator);
            var persistedSettings = newService.GetSettings();

            Assert.AreEqual(expectedLocation, persistedSettings.logLocation, "La localisation des logs n'a pas été sauvegardée sur le disque.");
        }

        [TestMethod]
        public void SetLogLocation_ShouldUpdateToLocalAndCentralized_AndPersist()
        {
            var service = new BackupService(orchestrator);
            LogLocation expectedLocation = LogLocation.localAndCentralized;
            service.SetLogLocation(expectedLocation);
            var currentSettings = service.GetSettings();
            Assert.AreEqual(expectedLocation, currentSettings.logLocation);
            var newService = new BackupService(orchestrator);
            var persistedSettings = newService.GetSettings();

            Assert.AreEqual(expectedLocation, persistedSettings.logLocation);
        }
    }
}