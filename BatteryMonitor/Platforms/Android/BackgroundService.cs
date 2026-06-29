using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using BatteryMonitor.Data;
using BatteryMonitor.Languages;
using BatteryMonitor.Shared;

namespace BatteryMonitor.Platforms.Android;

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeSpecialUse)]
internal class BackgroundService : Service
{
    const string ActionPeriodicCheck = "com.toyokenstudio.batterymonitor.ACTION_PERIODIC_CHECK";
    public const string ActionStopService = "com.toyokenstudio.batterymonitor.ACTION_STOP_SERVICE";

    int myId = (new object()).GetHashCode();
    int alertId = (new object()).GetHashCode();
    private readonly IBinder binder;
    int lowLevel = 0;
    int highLevel = 0;
    DateTime lastNotificationTime = DateTime.MinValue;
    BatteryChangedReceiver? batteryReceiver;
    AlarmManager? alarmManager;
    PendingIntent? alarmPendingIntent;
    bool isForeground;

    public BackgroundService()
    {
        binder = new LocalBinder(this);
    }

    public BackgroundService()
    {
        binder = new LocalBinder(this);
    }


    public class LocalBinder : Binder
    {
        private readonly BackgroundService _service;

        public LocalBinder(BackgroundService service)
        {
            _service = service;
        }

        public BackgroundService GetService() => _service;
    }


    public override IBinder? OnBind(Intent? intent)
    {
        return binder;
    }


    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        // User explicitly requested stop -> cancel alarm and shut down.
        if (intent?.Action == ActionStopService)
        {
            CancelAlarm();
            StopForeground(StopForegroundFlags.Remove);
            StopSelf();
            return StartCommandResult.NotSticky;
        }

        // Ensure we are always in foreground state.
        // Required both on first start AND when the alarm restarts the service
        // after the OS killed it (Android crashes the app if startForegroundService
        // is not followed by startForeground within 5 seconds).
        EnsureForeground();

        // Ensure battery receiver is registered (needed after OS-kill restart).
        RegisterBatteryReceiver();

        // If triggered by the safety-net alarm, check battery now.
        if (intent?.Action == ActionPeriodicCheck)
        {
            System.Diagnostics.Debug.WriteLine("Background service: alarm-triggered check.");
            CheckBattery();
        }
        else
        {
            // First start or Sticky restart after OS kill.
            System.Diagnostics.Debug.WriteLine("Background service started.");
        }

        // Always (re)schedule the next alarm.
        ScheduleNextAlarm();

        return StartCommandResult.Sticky;
    }


    /// <summary>
    /// Puts the service in foreground with a persistent notification.
    /// Only posts the notification once; subsequent calls are no-ops.
    /// </summary>
    private void EnsureForeground()
    {
        if (isForeground) return;

        var notificationIntent = new Intent(this, typeof(MainActivity));
        notificationIntent.SetAction("USER_TAPPED_NOTIFICATION");

        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        var notification = new NotificationCompat.Builder(this, MainApplication.ChannelIdService)
            .SetSmallIcon(Resource.Drawable.iconbattery32)
            .SetPriority(NotificationCompat.PriorityLow)
            .SetContentTitle(Strings.AppTitle)
            .SetContentText(Strings.ServiceStarted)
            .SetContentIntent(pendingIntent)
            .SetOngoing(true)
            .Build();

        StartForeground(myId, notification);
        isForeground = true;
    }


    private void ScheduleNextAlarm()
    {
        alarmManager ??= (AlarmManager?)GetSystemService(AlarmService);
        if (alarmManager == null) return;

        if (alarmPendingIntent == null)
        {
            var alarmIntent = new Intent(this, typeof(BackgroundService));
            alarmIntent.SetAction(ActionPeriodicCheck);
            alarmPendingIntent = PendingIntent.GetForegroundService(
                this, 0, alarmIntent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
        }

        var triggerAtMs = SystemClock.ElapsedRealtime() + Constants.TimerPeriod;
        alarmManager.SetAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, triggerAtMs, alarmPendingIntent);
    }


    private void CancelAlarm()
    {
        alarmManager ??= (AlarmManager?)GetSystemService(AlarmService);

        // Build a matching PendingIntent to cancel (in case the field was lost on process death).
        var alarmIntent = new Intent(this, typeof(BackgroundService));
        alarmIntent.SetAction(ActionPeriodicCheck);
        var pi = PendingIntent.GetForegroundService(
            this, 0, alarmIntent, PendingIntentFlags.Immutable | PendingIntentFlags.NoCreate);

        if (pi != null)
        {
            alarmManager?.Cancel(pi);
        }

        alarmPendingIntent = null;
    }


    private void RegisterBatteryReceiver()
    {
        if (batteryReceiver != null) return;

        batteryReceiver = new BatteryChangedReceiver(this);
        var filter = new IntentFilter(Intent.ActionBatteryChanged);
        RegisterReceiver(batteryReceiver, filter);
    }


    private void UnregisterBatteryReceiver()
    {
        if (batteryReceiver != null)
        {
            UnregisterReceiver(batteryReceiver);
            batteryReceiver = null;
        }
    }


    public override void OnDestroy()
    {
        // Do NOT cancel the alarm here. If the OS killed us, the alarm is the
        // safety net that will restart the service on the next trigger.
        // The alarm is only cancelled via ActionStopService (user-initiated).
        UnregisterBatteryReceiver();
        _ = AppLogService.Instance.LogAsync("Background service stopped.");
        base.OnDestroy();
    }


    private class BatteryChangedReceiver : BroadcastReceiver
    {
        private readonly BackgroundService service;

        public BatteryChangedReceiver(BackgroundService service)
        {
            this.service = service;
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent?.Action == Intent.ActionBatteryChanged)
            {
                service.CheckBattery();
            }
        }
    }


    private void CheckBattery()
    {
        try
        {
#if DEBUG
            MainThread.BeginInvokeOnMainThread(() =>
                Toast.MakeText(Platform.AppContext, "Battery Monitor checking...", ToastLength.Short)?.Show()
            );
#endif

            var batteryLevel = BatteryUtility.GetBatteryLevel();
            var batteryState = BatteryUtility.GetBatteryStatus();

            lowLevel = Preferences.Default.Get(Constants.MIN_VALUE, DefaultSettings.LowLevelWarningValue);
            highLevel = Preferences.Default.Get(Constants.MAX_VALUE, DefaultSettings.HighLevelWarningValue);

            var cooldownMinutes = Preferences.Default.Get(Constants.NOTIFICATION_COOLDOWN, DefaultSettings.NotificationCooldownMinutes);
            var now = DateTime.UtcNow;
            var elapsedTime = now - lastNotificationTime;
            var cooldownElapsed = elapsedTime.TotalMinutes >= cooldownMinutes;

            _ = AppLogService.Instance.LogAsync($"Battery check. Level: {batteryLevel}%, State: {batteryState}, Cooldown elapsed: {cooldownElapsed}.");

            if (batteryLevel <= lowLevel && batteryState != BatteryState.Charging)
            {
                if (cooldownElapsed)
                {
                    var text = $"{Strings.WarningLowLevel} ({batteryLevel}%)";
                    SendAlertNotification(text);
                    lastNotificationTime = now;
                    _ = AppLogService.Instance.LogAsync($"Low battery notification shown: {text}");
                }
                else
                {
                    _ = AppLogService.Instance.LogAsync($"Low battery notification bypassed due to cooldown (elapsed: {elapsedTime.TotalSeconds:F0}s, required: {cooldownMinutes}min).");
                }
            }

            if (batteryLevel >= highLevel && batteryState == BatteryState.Charging)
            {
                if (cooldownElapsed)
                {
                    var text = $"{Strings.WarningHighLevel} ({batteryLevel}%)";
                    SendAlertNotification(text);
                    lastNotificationTime = now;
                    _ = AppLogService.Instance.LogAsync($"High battery notification shown: {text}");
                }
                else
                {
                    _ = AppLogService.Instance.LogAsync($"High battery notification bypassed due to cooldown (elapsed: {elapsedTime.TotalSeconds:F0}s, required: {cooldownMinutes}min).");
                }
            }
        }
        catch (Exception ex)
        {
            _ = AppLogService.Instance.LogAsync($"CheckBattery error: {ex.Message}");
        }
    }


    private void SendAlertNotification(string text)
    {
        var notificationIntent = new Intent(this, typeof(MainActivity));
        notificationIntent.SetAction("USER_TAPPED_NOTIFICATION");
        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

        var alertNotification = new NotificationCompat.Builder(this, MainApplication.ChannelIdAlerts)
            .SetSmallIcon(Resource.Drawable.iconbattery32)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetDefaults((int)NotificationDefaults.All)
            .SetContentTitle(Strings.Notification_Title)
            .SetContentText(text)
            .SetContentIntent(pendingIntent)
            .SetAutoCancel(true)
            .Build();

        NotificationManagerCompat.From(this).Notify(alertId, alertNotification);
    }

    public override void OnDestroy()
    {
        try
        {
            timer?.Dispose();
            timer = null;
        }
        catch { }

        try
        {
            StopForeground(true);
        }
        catch { }

        AndroidServiceManager.IsRunning = false;
        Preferences.Default.Set(Constants.SERVICE_RUNNING, false);

        base.OnDestroy();
    }
}
