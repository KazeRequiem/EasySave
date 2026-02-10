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
    public class BackupSettingsRepository
    {
        private readonly string filePath;
        private static readonly object _lock = new object();

        public BackupSettingsRepository()
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        }

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
