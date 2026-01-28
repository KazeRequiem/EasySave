using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasySave.ViewModels;
using EasySave.Models;
using System;
using System.IO;
using System.Linq;

namespace EasySave.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class MainViewModelTests
    {
        private string _jsonPath;

        [TestInitialize]
        public void Setup()
        {
            _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jobs.json");
            try
            {
                if (File.Exists(_jsonPath)) File.Delete(_jsonPath);
            }
            catch (IOException) { }
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (File.Exists(_jsonPath)) File.Delete(_jsonPath);
            }
            catch (IOException) { }
        }

        [TestMethod]
        public void CreateJob_ShouldCreateJob_WhenDataIsValid()
        {
            var viewModel = new MainViewModel();
            string validSource = AppDomain.CurrentDomain.BaseDirectory;

            viewModel.CreateJob("ValidJob", validSource, "Dest", BackupType.Full);

            Assert.AreEqual(1, viewModel.BackupJobs.Count);
            Assert.AreEqual("ValidJob", viewModel.BackupJobs[0].Name);
        }

        [TestMethod]
        public void CreateJob_ShouldNotCreateJob_WhenNameIsEmpty()
        {
            var viewModel = new MainViewModel();
            string validSource = AppDomain.CurrentDomain.BaseDirectory;

            viewModel.CreateJob("", validSource, "Dest", BackupType.Full);

            Assert.AreEqual(0, viewModel.BackupJobs.Count);
        }

        [TestMethod]
        public void CreateJob_ShouldNotCreateJob_WhenSourceDirectoryDoesNotExist()
        {
            var viewModel = new MainViewModel();
            string invalidSource = @"C:\Dossier\Qui\Nexiste\Pas\Imaginaire";

            viewModel.CreateJob("JobName", invalidSource, "Dest", BackupType.Full);

            Assert.AreEqual(0, viewModel.BackupJobs.Count);
        }

        [TestMethod]
        public void ModifyJob_ShouldUpdateJob_WhenDataIsValid()
        {
            var viewModel = new MainViewModel();
            string validSource = AppDomain.CurrentDomain.BaseDirectory;

            viewModel.CreateJob("Original", validSource, "Dest", BackupType.Full);

            viewModel.ModifyJob(1, "Modified", validSource, "NewDest", BackupType.Differential);

            Assert.AreEqual("Modified", viewModel.BackupJobs[0].Name);
            Assert.AreEqual("NewDest", viewModel.BackupJobs[0].DestinationPath);
            Assert.AreEqual(BackupType.Differential, viewModel.BackupJobs[0].Type);
        }

        [TestMethod]
        public void DeleteJob_ShouldRemoveJob_WhenIdExists()
        {
            var viewModel = new MainViewModel();
            string validSource = AppDomain.CurrentDomain.BaseDirectory;
            viewModel.CreateJob("JobToDelete", validSource, "Dest", BackupType.Full);

            viewModel.DeleteJob(1);

            Assert.AreEqual(0, viewModel.BackupJobs.Count);
        }

        [TestMethod]
        public void ExecuteJob_ShouldCopyFiles_WhenJobExists()
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

            int jobId = viewModel.BackupJobs[0].Id;

            viewModel.ExecuteJob(jobId);

            string destFile = Path.Combine(destDir, "monFichier.txt");
            Assert.IsTrue(File.Exists(destFile), "Le fichier n'a pas été copié vers la destination.");

            if (Directory.Exists(sourceDir)) Directory.Delete(sourceDir, true);
            if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
        }
    }
}