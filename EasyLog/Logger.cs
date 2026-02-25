using System.Text.Json;
using System.Text;
using System.Xml.Linq;


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

        // Remplace ta fonction par celle-ci dans Logger.cs
        public void WriteLog(LogEntry entry)
        {
            string extension = entry.formatJsonOrXml == LogFormat.Json ? "json" : "xml";
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.{extension}";
            string filePath = Path.Combine(_directoryPath, fileName);

            try
            {
                // --- CORRECTION ICI : On vérifie si on doit écrire en LOCAL ---
                // 0 = local, 2 = localAndCentralized
                if (entry.location == LogLocation.local || entry.location == LogLocation.localAndCentralized)
                {
                    lock (_lock)
                    {
                        if (entry.formatJsonOrXml == LogFormat.Json)
                        {
                            string jsonLine = JsonSerializer.Serialize(entry);
                            File.AppendAllText(filePath, jsonLine + Environment.NewLine);
                        }
                        else if (entry.formatJsonOrXml == LogFormat.Xml)
                        {
                            XDocument doc = File.Exists(filePath) ? XDocument.Load(filePath) : new XDocument(new XElement("Logs"));
                            XElement newEntry = new XElement("LogEntry",
                                new XElement("time", entry.time),
                                new XElement("operationName", entry.operationName),
                                new XElement("savetype", entry.savetype),
                                new XElement("sourcePath", entry.sourcePath),
                                new XElement("destinationPath", entry.destinationPath),
                                new XElement("sizeFile", entry.sizeFile),
                                new XElement("timeTransfer", entry.timeTransfer),
                                new XElement("encryptionTimeMs", entry.encryptionTimeMs),
                                new XElement("success_Error", entry.success_Error)
                            );
                            doc.Root?.Add(newEntry);
                            doc.Save(filePath);
                        }
                    }
                }

                // --- ON VÉRIFIE SI ON DOIT ENVOYER AU DOCKER ---
                // 1 = centralized, 2 = localAndCentralized
                if (entry.location == LogLocation.centralized || entry.location == LogLocation.localAndCentralized)
                {
                    _ = SendToDocker(entry);
                }
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
                var response = await client.PostAsync("http://localhost:5000/api/logs", content);

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