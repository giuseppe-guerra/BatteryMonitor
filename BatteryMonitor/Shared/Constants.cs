namespace BatteryMonitor.Shared;

public static class Constants
{
    public const string MIN_VALUE = "MinValue";
    public const string MAX_VALUE = "MaxValue";

    public const string SERVICE_RUNNING = "ServiceRunning";

    public const string NOTIFICATION_COOLDOWN = "NotificationCooldown";

    public const string LOG_ENABLED = "LogEnabled";

    public const string APP_THEME = "AppTheme";

#if DEBUG
    public const double BATTERY_LEVEL_VIEW_INFO_TIME_PERIOD = 15000; // intervallo di aggiornamento della view
    public static int TimerPeriod = 60000;                           // Safety-net alarm interval (1 min debug)
#else
    public const double BATTERY_LEVEL_VIEW_INFO_TIME_PERIOD = 60000;
    public static int TimerPeriod = 120000;  // Safety-net alarm interval (2 min release)
#endif

    public const string IconFont_Home = "\uf015";
    public const string IconFont_Settings = "\uf013";
    public const string IconFont_About = "\uf05a";
    public const string IconFont_Sun = "\uf185";
    public const string IconFont_Moon = "\uf186";
    public const string IconFont_Log = "\uf15c";

    public const string GITHUB_URL = "https://github.com/giuseppe-guerra/BatteryMonitor";
    public const string EMAIL = "toyoken-studio@outlook.it";
}
