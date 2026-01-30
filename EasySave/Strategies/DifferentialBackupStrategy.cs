using EasySave.Models;
using EasySave.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasySave.Strategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(string sourcePath, string destinationPath, BackupState state, BackupStateRepository stateRepo)
        {
            var sourceDir = new DirectoryInfo(sourcePath);
            var allFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories);

            var filesToCopy = new List<FileInfo>();

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
        }
    }
}