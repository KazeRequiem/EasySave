namespace EasyLog
{
    /// <summary>
    /// Represents a log entry recorded by the logging system.
    /// 
    /// This model stores detailed information about an operation,
    /// including timestamps, file paths, transferred size,
    /// execution time, and success or error status.
    /// </summary>
    public class LogEntry
    {
        public string Time { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public string OperationName { get; set; }

        public required string NameSave { get; set; }

        public required string SourcePath { get; set; }

        public required string DestinationPath { get; set; }

        public long SizeFile { get; set; }

        public double TimeTransfer { get; set; }

        public string Success_Error { get; set; }
    }
}
