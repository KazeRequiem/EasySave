using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Strategies
{
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(string SourcePath, string DestinationPath)
        {
            Directory.CreateDirectory(DestinationPath);
            var dir = new DirectoryInfo(SourcePath);
            foreach (var filePath in Directory.GetFiles(SourcePath))
            {
                var fileName = Path.GetFileName(filePath);
                var destFile = Path.Combine(DestinationPath, fileName);
                if (!File.Exists(destFile) || File.GetLastWriteTime(filePath) > File.GetLastWriteTime(destFile))
                {
                    File.Copy(filePath,destFile, true);
                }
            }
            DirectoryInfo[] subDirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs)
            {
                string newDestinationPath = Path.Combine(DestinationPath, subDir.Name);
                Execute(subDir.FullName, newDestinationPath);
            }
        }
    }
}
