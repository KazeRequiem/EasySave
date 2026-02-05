using EasySave.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EasySave.Repositories
{
    /// <summary>
    /// Repository responsible for persisting and managing backup states.
    /// 
    /// This class handles reading and writing backup execution states
    /// to a JSON file in a thread-safe manner.
    /// It allows tracking the progress and status of running backups.
    /// </summary>
    public class BackupStateRepository
    {
        private readonly string filePath;
        private static readonly object _lock = new object();

        public BackupStateRepository()
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state.json");
        }

        /// <summary>
        /// Reads all backup states from persistent storage.
        /// 
        /// If the state file does not exist or cannot be read,
        /// an empty list is returned.
        /// </summary>
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

        /// <summary>
        /// Creates or updates a backup state in persistent storage.
        /// 
        /// If a state with the same name already exists, it is updated.
        /// Otherwise, a new state entry is added.
        /// </summary>
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
