using CommunityToolkit.Mvvm.ComponentModel;


namespace BatteryMonitor.ViewModel;

public partial class BatteryMonitorViewModel : ObservableObject
{
    [ObservableProperty]
    private int batteryPercentage;

    [ObservableProperty]
    private int minLimit;

    [ObservableProperty]
    private int maxLimit;

    [ObservableProperty]
    private int notificationCooldownMinutes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotServiceRunning))]
    private bool isServiceRunning;

    [ObservableProperty]
    private string serviceStatus;

    [ObservableProperty]
    private bool isLoggingEnabled;

    public bool IsNotServiceRunning => !IsServiceRunning;
}
