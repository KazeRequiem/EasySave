namespace EasyLog;

public class LogEntry
{

    public string Time { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    public required string NameSave { get; set; }

    public required string AddrSource { get; set; }

    public required string AddrDestination { get; set; }

    public long SizeFile { get; set; }

    public long TimeTransfer { get; set; }
}