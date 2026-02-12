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
    /// Implements the full backup strategy.
    /// 
    /// This strategy copies every file from the source directory
    /// to the destination directory, including all subdirectories.
    /// 
    /// It updates the backup state throughout the process
    /// (progression, current file paths, remaining files, timestamps)
    /// via the provided state repository.
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Executes a full backup operation from the source path to the destination path.
        /// 
        /// All files found in the source directory (recursively) are copied to the destination,
        /// preserving the relative directory structure.
        /// 
        /// The backup state is initialized before copying and updated after each file
        /// to reflect progress and current activity.
        /// </summary>
        public void Execute(string sourcePath, string destinationPath, BackupState state, BackupStateRepository stateRepo)
        {
            var settingsRepo = new BackupSettingsRepository();
            Settings settings = settingsRepo.ReadSettings();

            List<string> extensionsToEncrypt = settings.extensionsToEncrypt;
            string cryptoPath = settings.cryptoSoftPath;
            string cryptoKey = settings.cryptoKey;

            var sourceDir = new DirectoryInfo(sourcePath);
            var allFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories);

            state.totalFilesToCopy = allFiles.Length;
            state.totalFilesSize = allFiles.Sum(f => f.Length);
            state.nbFilesLeftToDo = state.totalFilesToCopy;
            state.progression = 0;
            state.state = "ACTIVE";
            stateRepo.UpdateState(state);

            foreach (var file in allFiles)
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

                    int encryptionTime = 0;
                    if (ShouldEncrypt(file.Extension, extensionsToEncrypt, cryptoPath))
                    {
                        encryptionTime = RunCryptoSoft(destFile, cryptoPath, cryptoKey);
                    }
                    if (encryptionTime < 0)
                    {
                        encryptionTime = -1;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying/encrypting file {file.Name}: {ex.Message}");
                }

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