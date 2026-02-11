using System;
using System.IO;
using System.Text.Json;
using EasyLog;
using System.Net.Http; 
using System.Text;     
using System.Threading.Tasks;

namespace EasyLog
{
    /// <summary>
    /// Provides a centralized logging system for the application.
    /// 
    /// This class implements a thread-safe Singleton pattern
    /// to ensure a single logging instance is used throughout
    /// the application lifecycle.
    /// 
    /// Logs are written in JSON format and stored in a dedicated
    /// directory, with one file per day.
    /// </summary>
    public sealed class Logger
    {
        private static readonly Lazy<Logger> _instance = new(() => new Logger());
        private readonly string _directoryPath;
        private readonly object _lock = new object();

        private Logger()
        {
            string rootPath = GetProjectRoot("EasySave");
            _directoryPath = Path.Combine(rootPath, "Logs");

            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }
        }

        /// <summary>
        /// Gets the single instance of the logger.
        /// 
        /// Ensures lazy and thread-safe initialization.
        /// </summary>
        public static Logger Instance => _instance.Value;

        /// <summary>
        /// Locates the root directory of the project based on a target folder name.
        /// 
        /// Traverses parent directories until the specified folder is found.
        /// If not found, falls back to the application base directory.
        /// </summary>
        private string GetProjectRoot(string targetFolder)
        {
            string? currentDir = AppDomain.CurrentDomain.BaseDirectory;

            while (currentDir != null)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(currentDir);

                if (dirInfo.Name.Equals(targetFolder, StringComparison.OrdinalIgnoreCase))
                {
                    return currentDir;
                }

                currentDir = Path.GetDirectoryName(currentDir);
            }

            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Writes a log entry to the daily log file.
        /// 
        /// Log entries are serialized in JSON format
        /// and appended to a file named with the current date.
        /// 
        /// This operation is thread-safe.
        /// </summary>
        public void WriteLog(LogEntry entry)
        {
            try
            {
                lock (_lock)
                {
                    string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
                    string filePath = Path.Combine(_directoryPath, fileName);
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(entry, options);
                    File.AppendAllText(filePath, jsonString + Environment.NewLine);
                }
                _ = SendToDocker(entry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logger Error] : {ex.Message}");
            }
        }
        private async Task SendToDocker(LogEntry entry)
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                var json = JsonSerializer.Serialize(entry);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://10.162.129.111:5000/api/logs", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[Docker Remote] Log envoyé au serveur central.");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[Docker Warning] Serveur distant injoignable. Backup local uniquement.");
            }
        }
    }
}


