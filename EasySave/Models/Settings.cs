using System.Collections.Generic;


namespace EasySave.Models
{
    public class Settings
    {
        public string cryptoSoftPath { get; set; }
        public string cryptoKey { get; set; }
        public List<string> extensionsToEncrypt { get; set; }
        public BackupLogType logType { get; set; }
        public string applicationSoftware { get; set; }

        public Settings()
        {
            cryptoSoftPath = "";
            cryptoKey = "";
            extensionsToEncrypt = new List<string>();
            logType = BackupLogType.json;
            applicationSoftware = "";
        }
    }
}
