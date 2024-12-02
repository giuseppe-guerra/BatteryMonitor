namespace BatteryMonitor.Shared;

public static class Constants
{
    public const string MIN_VALUE = "MinValue";
    public const string MAX_VALUE = "MaxValue";

    public const string SERVICE_RUNNING = "ServiceRunning";

#if DEBUG
    public const double BATTERY_LEVEL_VIEW_INFO_TIME_PERIOD = 15000; // intervallo di aggiornamento della view
    public static int TimerPeriod = 15000;                           // Intervallo di check del servizio
#else
    public const double BATTERY_LEVEL_VIEW_INFO_TIME_PERIOD = 60000;
    public static int TimerPeriod = 30000;  // Intervallo di check del servizio
#endif

    public const string IconFont_Home = "\uf015";
    public const string IconFont_Settings = "\uf013";
    public const string IconFont_About = "\uf05a";

    public const string GITHUB_URL = "https://github.com/giuseppe-guerra/BatteryMonitor";
    public const string EMAIL = "toyoken-studio@outlook.it";
}
