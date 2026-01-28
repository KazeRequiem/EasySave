using System;
using System.IO;
using System.Text.Json;

namespace EasyLog
{
    public static class Logger

        private static readonly string _directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

    private static readonly object _lock = new object();

    public static void WriteLog(LogEntry entry)
    {
        try
        {
            lock (_lock)
            {
                // 1. Créer le dossier Logs dans C:\Apps\MonProjet\ s'il n'existe pas
                if (!Directory.Exists(_directoryPath))
                    Directory.CreateDirectory(_directoryPath);

                // 2. Nom du fichier : 2023-10-27.json
                string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
                string filePath = Path.Combine(_directoryPath, fileName);

                // 3. Sérialisation
                string jsonString = JsonSerializer.Serialize(entry);

                // 4. Écriture (Ajoute une ligne à chaque log)
                File.AppendAllText(filePath, jsonString + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            // Sécurité pour ne pas planter l'application si le disque est protégé
            Console.WriteLine($"[Logger Error] {ex.Message}");
        }
    }
}
}