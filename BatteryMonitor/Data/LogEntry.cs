using SQLite;

namespace BatteryMonitor.Data;

public class LogEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public DateTime Timestamp { get; set; }

    public string Message { get; set; } = string.Empty;
}
