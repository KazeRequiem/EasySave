using EasyLog;
using EasySave.Models;
using EasySave.Repositories;
using EasySave.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace EasySave.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class MainViewModelTests
    {
        private string jsonPath;
        private string statePath;

        [TestInitialize]
        public void Setup()
        {
            jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");
            statePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state.json");
            try
            {
                if (File.Exists(jsonPath)) File.Delete(jsonPath);
                if (File.Exists(statePath)) File.Delete(statePath);
            }
            catch (Exception) { }
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (File.Exists(jsonPath)) File.Delete(jsonPath);
            }
            catch (IOException) { }
        }

        [TestMethod]
        public void CreateJob_ShouldCreateJob_WhenDataIsValid()
        {
            var viewModel = new MainViewModel();
            string validSource = AppDomain.CurrentDomain.BaseDirectory;

            viewModel.CreateJob("ValidJob", validSource, "Dest", BackupType.Full);

            Assert.AreEqual(1, viewModel.backupJobs.Count);
            Assert.AreEqual("ValidJob", viewModel.backupJobs[0].name);
        }

        [TestMethod]
        public void CreateJob_ShouldNotCreateJob_WhenNameIsEmpty()
        {
            var viewModel = new MainViewModel();
            string validSource = AppDomain.CurrentDomain.BaseDirectory;

            viewModel.CreateJob("", validSource, "Dest", BackupType.Full);

            Assert.AreEqual(0, viewModel.backupJobs.Count);
        }

        [TestMethod]
        public void CreateJob_ShouldNotCreateJob_WhenSourceDirectoryDoesNotExist()
        {
            var viewModel = new MainViewModel();
            string invalidSource = @"C:\Dossier\Qui\Nexiste\Pas\Imaginaire";

            try
            {
                viewModel.CreateJob("JobName", invalidSource, "Dest", BackupType.Full);
                Assert.Fail("Une DirectoryNotFoundException aurait dû être levée.");
            }
            catch (DirectoryNotFoundException)
            {
                Assert.AreEqual(0, viewModel.backupJobs.Count);
            }
        }

        [TestMethod]
        public void ModifyJob_ShouldUpdateJob_WhenDataIsValid()
        {
            var viewModel = new MainViewModel();
            string validSource = AppDomain.CurrentDomain.BaseDirectory;

            viewModel.CreateJob("Original", validSource, "Dest", BackupType.Full);

            viewModel.ModifyJob(1, "Modified", validSource, "NewDest", BackupType.Differential);

            Assert.AreEqual("Modified", viewModel.backupJobs[0].name);
            Assert.AreEqual("NewDest", viewModel.backupJobs[0].destinationPath);
            Assert.AreEqual(BackupType.Differential, viewModel.backupJobs[0].type);
        }

        [TestMethod]
        public void DeleteJob_ShouldRemoveJob_WhenIdExists()
        {
            var viewModel = new MainViewModel();
            string validSource = AppDomain.CurrentDomain.BaseDirectory;
            viewModel.CreateJob("JobToDelete", validSource, "Dest", BackupType.Full);

            viewModel.DeleteJob(1);

            Assert.AreEqual(0, viewModel.backupJobs.Count);
        }

        [TestMethod]
        public async Task ExecuteJob_ShouldCopyFiles_WhenJobExists()
        {
            var viewModel = new MainViewModel();

            string sourceDir = Path.Combine(Path.GetTempPath(), "EasySave_Source_Test");
            string destDir = Path.Combine(Path.GetTempPath(), "EasySave_Dest_Test");
            string filePath = Path.Combine(sourceDir, "monFichier.txt");

            if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
            if (Directory.Exists(destDir)) Directory.Delete(destDir, true);

            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(filePath, "Ceci est un test de sauvegarde.");

            viewModel.CreateJob("JobExecution", sourceDir, destDir, BackupType.Full);

            int jobId = viewModel.backupJobs[0].id;

            await viewModel.ExecuteJob(jobId);

            string destFile = Path.Combine(destDir, "monFichier.txt");
            Assert.IsTrue(File.Exists(destFile), "Le fichier n'a pas été copié vers la destination.");

            if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
            if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
        }

        [TestMethod]
        public void ConfigureSettings_ShouldUpdateSettings_WhenDataIsValid()
        {
            var viewModel = new MainViewModel();
            string expectedKey = "MySecretKey";
            string expectedApp = "Notepad.exe";
            string expectedExe = ".txt";
            LogFormat expectedLogType = LogFormat.Json;

            viewModel.UpdateCryptKey(expectedKey);
            viewModel.UpdateApplicationSoftware(expectedApp);
            viewModel.AddEncryptionExtension(expectedExe);
            viewModel.UpdateLogType("Json");

            var repo = new BackupSettingsRepository();
            var settings = repo.ReadSettings();

            Assert.AreEqual(expectedKey, settings.cryptoKey);
            Assert.AreEqual(expectedApp, settings.applicationSoftware);
            Assert.AreEqual(expectedLogType, settings.logType);
            Assert.IsTrue(settings.extensionsToEncrypt.Contains(".txt"));
        }


        [TestMethod]
        public void ControlMethods_ShouldExecuteWithoutExceptions()
        {
            var viewModel = new MainViewModel();
            string validSource = AppDomain.CurrentDomain.BaseDirectory;
            viewModel.CreateJob("Job1", validSource, "Dest", BackupType.Full);

            try
            {
                viewModel.PauseJob();
                viewModel.ResumeJob();
                viewModel.StopJob(1);
                viewModel.StopAllJobs();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Une méthode de contrôle a levé une exception inattendue : {ex.Message}");
            }
        }

        [TestMethod]
        public void UpdateCryptPath_ShouldUpdatePathAndPersist()
        {
            var viewModel = new MainViewModel();
            string expectedPath = @"C:\Softwares\Crypto.exe";

            viewModel.UpdateCryptPath(expectedPath);

            var settings = viewModel.GetCurrentSetting();
            Assert.AreEqual(expectedPath, settings.cryptoSoftPath);
        }

        [TestMethod]
        public void SetMaxFileSize_ShouldUpdateSize_WhenPositive()
        {
            var viewModel = new MainViewModel();
            int newSize = 50000;

            viewModel.SetMaxFileSize(newSize);

            var settings = viewModel.GetCurrentSetting();
            Assert.AreEqual(newSize, settings.maxFileSizeKo);
        }

        [TestMethod]
        public void SetMaxFileSize_ShouldNotUpdate_WhenSizeIsNegative()
        {
            var viewModel = new MainViewModel();
            viewModel.SetMaxFileSize(50000);

            viewModel.SetMaxFileSize(-10); 

            var settings = viewModel.GetCurrentSetting();
            Assert.AreEqual(50000, settings.maxFileSizeKo);
        }


        [TestMethod]
        public void EncryptionExtensions_ShouldAddAndRemoveCorrectly()
        {
            var viewModel = new MainViewModel();

            viewModel.AddEncryptionExtension("xml");
            Assert.IsTrue(viewModel.GetCurrentSetting().extensionsToEncrypt.Contains(".xml"));

            viewModel.RemoveEncryptionExtension(".xml"); 
            Assert.IsFalse(viewModel.GetCurrentSetting().extensionsToEncrypt.Contains(".xml"));
        }

        [TestMethod]
        public void PriorityExtensions_ShouldAddAndRemoveCorrectly()
        {
            var viewModel = new MainViewModel();

            viewModel.AddPriorityExtension("iso");
            Assert.IsTrue(viewModel.GetCurrentSetting().priorityExtensions.Contains(".iso"));

            viewModel.RemovePriorityExtension(".iso");
            Assert.IsFalse(viewModel.GetCurrentSetting().priorityExtensions.Contains(".iso"));
        }


        [TestMethod]
        public void GetGlobalProgress_ShouldReturnZero_WhenNoStates()
        {
            var viewModel = new MainViewModel();
            double progress = viewModel.GetGlobalProgress();

            Assert.AreEqual(0, progress);
        }

        [TestMethod]
        public void SetLogLocation_ShouldUpdateToLocal_WhenInputIsLocal()
        {
            var viewModel = new MainViewModel();
            viewModel.SetLogLocation("local");
            var settings = viewModel.GetCurrentSetting();
            Assert.AreEqual(LogLocation.local, settings.logLocation);
        }

        [TestMethod]
        public void SetLogLocation_ShouldBeCaseInsensitive_WhenInputIsLocal()
        {
            var viewModel = new MainViewModel();
            viewModel.SetLogLocation("LOCAL");
            var settings = viewModel.GetCurrentSetting();
            Assert.AreEqual(LogLocation.local, settings.logLocation);
        }

        [TestMethod]
        public void SetLogLocation_ShouldUpdateToCentralized_WhenInputIsCentralized()
        {
            var viewModel = new MainViewModel();
            viewModel.SetLogLocation("centralized");
            var settings = viewModel.GetCurrentSetting();
            Assert.AreEqual(LogLocation.centralized, settings.logLocation);
        }

        [TestMethod]
        public void SetLogLocation_ShouldUpdateToLocalAndCentralized_WhenInputIsFallback()
        {
            var viewModel = new MainViewModel();
            viewModel.SetLogLocation("random");
            var settings = viewModel.GetCurrentSetting();
            Assert.AreEqual(LogLocation.localAndCentralized, settings.logLocation);
        }
    }
}