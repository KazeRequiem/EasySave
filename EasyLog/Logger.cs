using System;
using System.IO;
using System.Text.Json;

namespace EasyLog
{
    public sealed class Logger
    {
        private static readonly Lazy<Logger> _instance = new(() => new Logger());
        private readonly string _directoryPath;
        private readonly object _lock = new object();

        private Logger()
        {
            // On cherche spécifiquement le dossier qui s'appelle "EasySave"
            string rootPath = GetProjectRoot("EasySave");

            _directoryPath = Path.Combine(rootPath, "Logs");
            Console.WriteLine("Voici le chemin d'accès : " + _directoryPath);

            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }
        }

        public static Logger Instance => _instance.Value;

        // Note : Ajout du paramètre 'targetFolder' ici pour corriger l'erreur CS1501
        private string GetProjectRoot(string targetFolder)
        {
            string? currentDir = AppDomain.CurrentDomain.BaseDirectory;

            while (currentDir != null)
            {
                // On récupère les infos du dossier actuel
                DirectoryInfo dirInfo = new DirectoryInfo(currentDir);

                // Si le nom du dossier est celui qu'on cherche ("EasySave")
                if (dirInfo.Name.Equals(targetFolder, StringComparison.OrdinalIgnoreCase))
                {
                    return currentDir;
                }

                // Sinon on remonte
                currentDir = Path.GetDirectoryName(currentDir);
            }

            return AppDomain.CurrentDomain.BaseDirectory;
        }

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
                    Console.WriteLine($"[Logger] Log écrit avec succès dans : {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logger Error] : {ex.Message}");
            }
        }
    }
}