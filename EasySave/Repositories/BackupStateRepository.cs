using EasySave.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EasySave.Repositories
{
    public class BackupStateRepository
    {
        private readonly string filePath;
        private static readonly object _lock = new object();

        public BackupStateRepository()
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state.json");
        }

        public List<BackupState> ReadStates()
        {
            lock (_lock)
            {
                if (!File.Exists(filePath)) return new List<BackupState>();
                try
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<BackupState>>(json) ?? new List<BackupState>();
                }
                catch { return new List<BackupState>(); }
            }
        }

        public void UpdateState(BackupState state)
        {
            lock (_lock)
            {
                var states = ReadStates();

                var existingStateIndex = states.FindIndex(s => s.name == state.name);

                if (existingStateIndex != -1)
                {
                    states[existingStateIndex] = state;
                }
                else
                {
                    states.Add(state);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(states, options);
                File.WriteAllText(filePath, json);
            }
        }
    }
}