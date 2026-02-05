
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
        public string time { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public string operationName { get; set; }

        public required string nameSave { get; set; }

        public required string sourcePath { get; set; }

        public required string destinationPath { get; set; }

        public long sizeFile { get; set; }

        public double timeTransfer { get; set; }

        public string success_Error { get; set; }
    }
}
