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
    [NotifyPropertyChangedFor(nameof(IsNotServiceRunning))]
    private bool isServiceRunning;

    [ObservableProperty]
    private string serviceStatus;

    public bool IsNotServiceRunning => !IsServiceRunning;
}
