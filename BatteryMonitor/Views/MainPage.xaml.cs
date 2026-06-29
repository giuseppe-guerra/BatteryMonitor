using BatteryMonitor.Data;
using BatteryMonitor.Languages;
using BatteryMonitor.Shared;
using BatteryMonitor.ViewModel;

namespace BatteryMonitor;

public partial class MainPage : ContentPage
{
    private BatteryMonitorViewModel batteryMonitorModel;
    private AppLogService _logService;
    IDispatcherTimer batteryLeveltimer = null!;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CheckNotificationPermission();
    }


    public MainPage(BatteryMonitorViewModel batteryMonitorModel, AppLogService logService)
    {
        InitializeComponent();

        this.batteryMonitorModel = batteryMonitorModel;
        _logService = logService;
        BindingContext = batteryMonitorModel;

        batteryLeveltimer = Dispatcher.CreateTimer();
        batteryLeveltimer.Interval = TimeSpan.FromMilliseconds(Constants.BATTERY_LEVEL_VIEW_INFO_TIME_PERIOD);
        batteryLeveltimer.Tick += BatteryLeveltimer_Tick;
        batteryLeveltimer.Start();

        GetPreferences();

#if ANDROID
        if (!BatteryMonitor.Platforms.Android.AndroidServiceManager.IsRunning)
        {
            batteryMonitorModel.ServiceStatus = Strings.NotRunning;
            batteryMonitorModel.IsServiceRunning = false;
        }
        else
        {
            batteryMonitorModel.ServiceStatus = Strings.Running;
            batteryMonitorModel.IsServiceRunning = true;
        }
#endif

        UpdateBatteryLevel();

        _ = _logService.LogAsync("App started.");
    }


    public void UpdateBatteryLevel()
    {
        batteryMonitorModel.BatteryPercentage = BatteryUtility.GetBatteryLevel();
    }


    private async void btnStartService_Clicked(object sender, EventArgs e)
    {
        await Task.Run(() => StartService());

        StoreServiceStatus(true);
        batteryMonitorModel.ServiceStatus = Strings.Running;
        batteryMonitorModel.IsServiceRunning = true;
        await _logService.LogAsync("Service started by the user");
    }


    private void BatteryLeveltimer_Tick(object? sender, EventArgs e)
    {
        UpdateBatteryLevel();
    }


    private async void btnStopService_Clicked(object sender, EventArgs e)
    {
        await Task.Run(() => StopService());

        StoreServiceStatus(false);
        batteryMonitorModel.ServiceStatus = Strings.NotRunning;
        batteryMonitorModel.IsServiceRunning = false;
        await _logService.LogAsync("Service stopped by the user");
    }


    private void GetPreferences()
    {
        batteryMonitorModel.MinLimit = Preferences.Default.Get(Constants.MIN_VALUE, DefaultSettings.LowLevelWarningValue);
        batteryMonitorModel.MaxLimit = Preferences.Default.Get(Constants.MAX_VALUE, DefaultSettings.HighLevelWarningValue);
        batteryMonitorModel.NotificationCooldownMinutes = Preferences.Default.Get(Constants.NOTIFICATION_COOLDOWN, DefaultSettings.NotificationCooldownMinutes);

        if (Preferences.Default.Get(Constants.SERVICE_RUNNING, false))
        {
            StartService();
        }
    }


    private async void CheckNotificationPermission()
    {
        if (await Permissions.RequestAsync<Permissions.PostNotifications>() != PermissionStatus.Granted)
        {
            await DisplayAlertAsync(Strings.AppTitle, Strings.PermissionDescription, "OK");
            return;
        }
    }

    private async Task StartService()
    {
#if ANDROID
        if (!BatteryMonitor.Platforms.Android.AndroidServiceManager.IsRunning) 
        {
            BatteryMonitor.Platforms.Android.AndroidServiceManager.StartMyService();
        }
#endif
    }

    private async Task StopService()
    {
#if ANDROID
        BatteryMonitor.Platforms.Android.AndroidServiceManager.StopMyService();
#endif
    }

    private void StoreServiceStatus(bool isServiceRunning)
    {
        Preferences.Default.Set(Constants.SERVICE_RUNNING, isServiceRunning);
    }
}
