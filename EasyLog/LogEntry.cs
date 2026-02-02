namespace EasyLog
{
    public class LogEntry
    {
        public string Time { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public string OperationName { get; set; }

        public required string NameSave { get; set; }

        public required string SourcePath { get; set; }

        public required string DestinationPath { get; set; }

        public long SizeFile { get; set; }

        public long TimeTransfer { get; set; }

        public string Status { get; set; }

        public string Succes_Error { get; set; }
    }
}