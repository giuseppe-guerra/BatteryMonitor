using SQLite;

namespace BatteryMonitor.Data;

public class AppLogService
{
    private const string DatabaseFilename = "applogs.db3";

    private static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    private SQLiteAsyncConnection? _database;

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        _database = new SQLiteAsyncConnection(DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _database.CreateTableAsync<LogEntry>();
        return _database;
    }

    public async Task LogAsync(string message)
    {
        var db = await GetDatabaseAsync();
        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Message = message
        };
        await db.InsertAsync(entry);
    }

    public async Task<List<LogEntry>> GetLogsAsync(int limit = 100)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<LogEntry>()
                       .OrderByDescending(l => l.Timestamp)
                       .Take(limit)
                       .ToListAsync();
    }

    public async Task ClearLogsAsync()
    {
        var db = await GetDatabaseAsync();
        await db.DeleteAllAsync<LogEntry>();
    }

    // Singleton instance for use outside DI (e.g., Android services)
    private static AppLogService? _instance;
    public static AppLogService Instance => _instance ??= new AppLogService();

    public static void SetInstance(AppLogService service) => _instance = service;
}
