using EasySave.Models;
using EasySave.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasySave.Strategies
{
    /// <summary>
    /// Implements the differential backup strategy.
    /// 
    /// This strategy copies only files that are new or have been modified
    /// since the last backup (based on the destination file existence and
    /// last write time comparison).
    /// 
    /// It updates the backup state throughout the process
    /// (progression, current file paths, remaining files, timestamps)
    /// via the provided state repository.
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Executes a differential backup operation from the source path to the destination path.
        /// 
        /// Only files that do not exist in the destination, or whose last write time
        /// is more recent than the destination version, are copied.
        /// 
        /// The backup state is initialized before copying and updated after each copied file
        /// to reflect progress and current activity.
        /// </summary>  
        public double Execute(string sourcePath, string destinationPath, BackupState state, BackupStateRepository stateRepo)
        {
            var settingsRepo = new BackupSettingsRepository();
            Settings settings = settingsRepo.ReadSettings();

            List<string> extensionsToEncrypt = settings.extensionsToEncrypt;
            string cryptoPath = settings.cryptoSoftPath;
            string cryptoKey = settings.cryptoKey;

            var sourceDir = new DirectoryInfo(sourcePath);
            var allFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories);

            var filesToCopy = new List<FileInfo>();

            double encryptionTime = 0;

            foreach (var file in allFiles)
            {
                string relativePath = Path.GetRelativePath(sourcePath, file.FullName);
                string destFile = Path.Combine(destinationPath, relativePath);

                if (!File.Exists(destFile) || file.LastWriteTime > File.GetLastWriteTime(destFile))
                {
                    filesToCopy.Add(file);
                }
            }

            state.totalFilesToCopy = filesToCopy.Count;
            state.totalFilesSize = filesToCopy.Sum(f => f.Length);
            state.nbFilesLeftToDo = state.totalFilesToCopy;
            state.progression = 0;
            state.state = "ACTIVE";
            stateRepo.UpdateState(state);

            foreach (var file in filesToCopy)
            {
                string relativePath = Path.GetRelativePath(sourcePath, file.FullName);
                string destFile = Path.Combine(destinationPath, relativePath);

                string? destDir = Path.GetDirectoryName(destFile);
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
                    File.Copy(file.FullName, destFile, true);

                    if (ShouldEncrypt(file.Extension, extensionsToEncrypt, cryptoPath))
                    {
                        encryptionTime = RunCryptoSoft(destFile, cryptoPath, cryptoKey);
                    }
                    if (encryptionTime <= 0)
                    {
                        encryptionTime = -1;

                    }
                }
                catch { }

                state.nbFilesLeftToDo--;

                int filesDone = state.totalFilesToCopy - state.nbFilesLeftToDo;
                if (state.totalFilesToCopy > 0)
                {
                    state.progression = (int)((double)filesDone / state.totalFilesToCopy * 100);
                }
                else
                {
                    state.progression = 100;
                }

                stateRepo.UpdateState(state);
            }
            return encryptionTime;

        }
        /// <summary>
        /// Verify if the extension is in the list and if CryptoSoft is loaded.
        /// </summary>
        private bool ShouldEncrypt(string extension, List<string> extensions, string cryptoPath)
        {
            if (extensions == null || extensions.Count == 0) return false;
            if (string.IsNullOrEmpty(cryptoPath) || !File.Exists(cryptoPath)) return false;
            return extensions.Contains(extension);
        }

        /// <summary>
        /// Launch CryptoSoft
        /// </summary>
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