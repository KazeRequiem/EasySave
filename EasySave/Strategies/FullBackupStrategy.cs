using EasySave.Models;
using EasySave.Orchestration;
using EasySave.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        private static readonly object _cryptoLock = new object();

        /// <summary>
        /// Executes a full backup operation asynchronously.
        /// </summary>
        public async Task<double> Execute(string sourcePath, string destinationPath, BackupState state, BackupStateRepository stateRepo, CancellationToken token, Orchestrator orchestrator)
        {
            double totalEncryptionTime = 0;
            var settingsRepo = new BackupSettingsRepository();
            Settings settings = settingsRepo.ReadSettings();

            List<string> extensionsToEncrypt = settings.extensionsToEncrypt;
            string cryptoPath = settings.cryptoSoftPath;
            string cryptoKey = settings.cryptoKey;

            var sourceDir = new DirectoryInfo(sourcePath);
            if (!sourceDir.Exists) throw new DirectoryNotFoundException(sourcePath);

            var allFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories)
                .OrderByDescending(f => orchestrator.IsPriority(f.Extension))
                .ToArray();

            state.totalFilesToCopy = allFiles.Length;
            state.totalFilesSize = allFiles.Sum(f => f.Length);
            state.nbFilesLeftToDo = state.totalFilesToCopy;
            state.progression = 0;
            state.state = "ACTIVE";
            stateRepo.UpdateState(state);

            foreach (var file in allFiles)
            {
                token.ThrowIfCancellationRequested();

                string relativePath = Path.GetRelativePath(sourcePath, file.FullName);
                string destFile = Path.Combine(destinationPath, relativePath);
                string? destDir = Path.GetDirectoryName(destFile);

                long fileSize = file.Length;
                bool isPriority = orchestrator.IsPriority(file.Extension);

                if (destDir != null && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                state.sourceFilePath = file.FullName;
                state.targetFilePath = destFile;
                state.lastActionTimestamp = DateTime.Now;
                stateRepo.UpdateState(state);

                try
                {
                    await orchestrator.AcquirePermissionAsync(fileSize, isPriority);

                    using (FileStream sourceStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (FileStream destStream = File.Create(destFile))
                    {
                        Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] - {DateTime.Now:HH:mm:ss.fff} : Démarrage copie de {file.Name} Taille : {fileSize}");
                        await sourceStream.CopyToAsync(destStream, token);
                    }

                    if (ShouldEncrypt(file.Extension, extensionsToEncrypt, cryptoPath))
                    {
                        double encryptionTime = 0;

                        lock (_cryptoLock)
                        {
                            encryptionTime = RunCryptoSoft(destFile, cryptoPath, cryptoKey);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw; 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file.Name}: {ex.Message}");
                    // Quentin log Action
                }
                finally
                {
                    orchestrator.ReleasePermission(fileSize);

                    if (isPriority) orchestrator.UnregisterPriorityFile();
                }
                state.nbFilesLeftToDo--;
                int filesDone = state.totalFilesToCopy - state.nbFilesLeftToDo;

                if (state.totalFilesToCopy > 0)
                    state.progression = (int)((double)filesDone / state.totalFilesToCopy * 100);
                else
                    state.progression = 100;

                stateRepo.UpdateState(state);
            }

            state.state = "END";
            state.progression = 100;
            stateRepo.UpdateState(state);
            return totalEncryptionTime;
        }

        private bool ShouldEncrypt(string extension, List<string> extensions, string cryptoPath)
        {
            if (extensions == null || extensions.Count == 0) return false;
            if (string.IsNullOrEmpty(cryptoPath) || !File.Exists(cryptoPath)) return false;
            return extensions.Contains(extension);
        }

        private int RunCryptoSoft(string filePath, string cryptoPath, string key)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = cryptoPath;
                p.StartInfo.Arguments = $"\"{filePath}\" \"{key}\"";

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;

                p.Start();
                p.WaitForExit();

                return p.ExitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CryptoSoft Error : {ex.Message}");
                return 0;
            }
        }
    }
}