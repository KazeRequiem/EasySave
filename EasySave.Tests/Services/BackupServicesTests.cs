using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasySave.Services;
using EasySave.Models;
using System;
using System.IO;
using System.Linq;

namespace EasySave.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class BackupServiceTests
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
        public void CreateJob_ShouldAddJobToList()
        {
            var service = new BackupService();
            service.CreateJob("Job1", "Source", "Dest", BackupType.Full);

            Assert.AreEqual(1, service.backupJobs.Count);
            Assert.AreEqual("Job1", service.backupJobs[0].name);
        }

        [TestMethod]
        public void CreateJob_ShouldThrowException_WhenMoreThan5Jobs()
        {
            var service = new BackupService();
            for (int i = 0; i < 5; i++)
            {
                service.CreateJob($"Job{i}", "Source", "Dest", BackupType.Full);
            }

            try
            {
                service.CreateJob("Job6", "Source", "Dest", BackupType.Full);
                Assert.Fail("An exception should have been raised");
            }
            catch (InvalidOperationException)
            {
            }
            catch (Exception ex)
            {
                Assert.Fail($"Wrong exception received : {ex.GetType()}");
            }
        }

        [TestMethod]
        public void DeleteJob_ShouldRemoveJobAndReorderIds()
        {
            var service = new BackupService();
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
            var service = new BackupService();
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
            var service = new BackupService();
            service.CreateJob("OriginalName", "SourceA", "DestA", BackupType.Full);

            service.ModifyJob(1, "UpdatedName", "SourceB", "DestB", BackupType.Differential);

            var job = service.backupJobs.FirstOrDefault(j => j.id == 1);
            Assert.IsNotNull(job);
            Assert.AreEqual("UpdatedName", job.name);
            Assert.AreEqual("SourceB", job.sourcePath);
            Assert.AreEqual(BackupType.Differential, job.type);

            var service2 = new BackupService();
            var persistedJob = service2.backupJobs.FirstOrDefault(j => j.id == 1);

            Assert.IsNotNull(persistedJob);
            Assert.AreEqual("UpdatedName", persistedJob.name);
        }

        [TestMethod]
        public void ExecuteJob_ShouldThrowException_WhenIdNotFound()
        {
            var service = new BackupService();

            try
            {
                service.ExecuteJob(1);
                Assert.Fail("An exception should have been raised");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void LoadJobs_ShouldLoadExistingJobsFromFile()
        {
            var service1 = new BackupService();
            service1.CreateJob("PersistentJob", "Source", "Dest", BackupType.Full);

            var service2 = new BackupService();

            Assert.AreEqual(1, service2.backupJobs.Count);
            Assert.AreEqual("PersistentJob", service2.backupJobs[0].name);
        }
    }
}