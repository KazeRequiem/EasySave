using EasySave.Models;
using EasySave.Repositories;
using System.IO;

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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying file {file.Name}: {ex.Message}");
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
    }
}
