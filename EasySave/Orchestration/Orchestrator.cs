using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Orchestration
{
    public class Orchestrator
    {
        private readonly long limitSizeBytes;
        private readonly List<string> priorityExtensions;
        private readonly SemaphoreSlim largeFileSemaphore;
        private int priorityFilesPendingCount = 0;

        private ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);

        private bool isStopped = false;

        public Orchestrator(long limitSizeKo, List<string> priorityExtensions)
        {
            this.limitSizeBytes = limitSizeKo * 1024;
            this.priorityExtensions = new List<string>();
            if (priorityExtensions != null)
            {
                foreach (var ext in priorityExtensions) this.priorityExtensions.Add(ext.ToLower());
            }
            this.largeFileSemaphore = new SemaphoreSlim(1, 1);
        }

        public void GlobalPause() => pauseEvent.Reset();
        public void GlobalResume() => pauseEvent.Set();
        public void GlobalStop() => isStopped = true;
        public bool IsPaused => !pauseEvent.IsSet;

        public void RegisterPriorityFile()
        {
            Interlocked.Increment(ref priorityFilesPendingCount);
        }

        public void UnregisterPriorityFile()
        {
            int current = Interlocked.Decrement(ref priorityFilesPendingCount);
            Console.WriteLine($"[ORCHESTRATOR] Fichier prio fini. Restants : {current}");
        }

        public bool IsPriority(string extension)
        {
            if (string.IsNullOrEmpty(extension)) return false;
            return priorityExtensions.Contains(extension.ToLower());
        }

        public async Task AcquirePermissionAsync(long fileSize, bool isPriority)
        {
            if (isStopped) throw new OperationCanceledException("Backup stopped by user.");
            pauseEvent.Wait();

            while (!isPriority && Interlocked.CompareExchange(ref priorityFilesPendingCount, 0, 0) > 0)
            {
                await Task.Delay(50);
                pauseEvent.Wait();
            }
            if (limitSizeBytes > 0 && fileSize > limitSizeBytes)
            {
                await largeFileSemaphore.WaitAsync();
            }
        }

        public void ReleasePermission(long fileSize)
        {
            if (limitSizeBytes > 0 && fileSize > limitSizeBytes)
            {
                try { largeFileSemaphore.Release(); } catch { }
            }
        }
    }
}