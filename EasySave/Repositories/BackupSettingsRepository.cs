using EasySave.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Xml;

namespace EasySave.Repositories
{
    /// <summary>
    /// Repository responsible for managing the application's global configuration.
    /// 
    /// This class handles the persistence of settings (business software, encryption, logs)
    /// to a JSON file. It ensures thread-safety using a locking mechanism.
    /// </summary>
    public class BackupSettingsRepository
    {
        private readonly string filePath;
        private static readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the repository and defines the path to the settings file.
        /// </summary>
        public BackupSettingsRepository()
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        }

        /// <summary>
        /// Reads the current settings from the JSON file.
        /// 
        /// If the file does not exist, a default configuration is created, saved, and returned.
        /// If an error occurs during reading, a default configuration instance is returned.
        /// </summary>
        /// <returns>The current application settings.</returns>    
        public Settings ReadSettings()
        {
            lock (_lock)
            {
                if (!File.Exists(filePath))
                {
                    var defaultSettings = new Settings();
                    WriteSettings(defaultSettings);
                    return defaultSettings;
                }
                try
                {
                    string json = File.ReadAllText(filePath);

                    var options = new JsonSerializerOptions();
                    options.Converters.Add(new JsonStringEnumConverter());

                    return JsonSerializer.Deserialize<Settings>(json, options) ?? new Settings();
                }
                catch
                {
                    return new Settings();
                }
            }
        }

        /// <summary>
        /// Writes the provided settings to the JSON file.
        /// 
        /// This operation is thread-safe and formats the JSON output for readability.
        /// </summary>
        /// <param name="settings">The settings object to persist.</param>
        public void WriteSettings(Settings settings)
        {
            lock (_lock)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                options.Converters.Add(new JsonStringEnumConverter());
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(filePath, json);
            }
        }
    }
}
