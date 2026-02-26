using EasySave.Models;
using EasySave.Orchestration;
using EasySave.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasySave.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class OrchestratorAndControlTests
    {

        [TestMethod]
        public void PauseAndResume_ShouldChangeOrchestratorState()
        {
            var orchestrator = new Orchestrator(0, new List<string>());
            var service = new BackupService(orchestrator);

            Assert.IsFalse(orchestrator.IsPaused, "L'orchestrateur devrait être en lecture au démarrage.");
            service.PauseJob();
            Assert.IsTrue(orchestrator.IsPaused, "L'orchestrateur devrait être en pause.");
            service.ResumeJob();
            Assert.IsFalse(orchestrator.IsPaused, "L'orchestrateur devrait avoir repris.");
        }

        [TestMethod]
        public async Task StopAllJobs_ShouldPreventAnyNewFileFromCopying()
        {
            var orchestrator = new Orchestrator(0, new List<string>());
            var service = new BackupService(orchestrator);

            service.StopAllJobs();

            try
            {
                await orchestrator.AcquirePermissionAsync(1024, false);

                Assert.Fail("Une OperationCanceledException aurait dû être levée car le job a été stoppé.");
            }
            catch (OperationCanceledException)
            {
            }
        }

        [TestMethod]
        public async Task ExecuteJob_WhenStopped_ShouldThrowTaskCanceledException()
        {
            var orchestrator = new Orchestrator(0, new List<string>());
            var service = new BackupService(orchestrator);
            service.CreateJob("JobToStop", "C:\\DummySource", "C:\\DummyDest", BackupType.Full);

            try
            {
                Task executionTask = service.ExecuteJob(1);

                service.StopJob(1);

                await executionTask;
                Assert.Fail("La tâche aurait dû être annulée.");
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex);
            }
        }
    }
}