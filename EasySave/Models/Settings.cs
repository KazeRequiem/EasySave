using EasyLog;
using System.Collections.Generic;


namespace EasySave.Models
{
    public class Settings
    {
        public string cryptoSoftPath { get; set; }
        public string cryptoKey { get; set; }
        public List<string> extensionsToEncrypt { get; set; }
        public LogFormat logType { get; set; }
        public string applicationSoftware { get; set; }
        public List<string> priorityExtensions { get; set; }
        public long maxFileSizeKo { get; set; }
        public LogLocation logLocation { get; set; }

        public Settings()
        {
            cryptoSoftPath = "";
            cryptoKey = "";
            extensionsToEncrypt = new List<string>();
            logType = LogFormat.Json;
            applicationSoftware = "";
            priorityExtensions = new List<string>();
            maxFileSizeKo = 10000;
            logLocation = LogLocation.local;
        }
    }
}
