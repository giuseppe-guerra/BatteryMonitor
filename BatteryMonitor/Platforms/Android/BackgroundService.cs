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
    int myId = (new object()).GetHashCode();
    int alertId = (new object()).GetHashCode();
    private readonly IBinder binder;
    int lowLevel = 0;
    int highLevel = 0;
    DateTime lastNotificationTime = DateTime.MinValue;
    BatteryChangedReceiver? batteryReceiver;
    Handler? handler;
    PowerManager.WakeLock? wakeLock;

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
        AcquireWakeLock();

        // Build the foreground notification
        var notificationIntent = new Intent(this, typeof(MainActivity));
        notificationIntent.SetAction("USER_TAPPED_NOTIFICATION");

        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

        var notificationBuilder = new NotificationCompat.Builder(this, MainApplication.ChannelIdService)
            .SetSmallIcon(Resource.Drawable.iconbattery32)
            .SetPriority(NotificationCompat.PriorityLow)
            .SetContentTitle(Strings.AppTitle)
            .SetContentText(Strings.ServiceStarted)
            .SetContentIntent(pendingIntent)
            .SetOngoing(true);

        StartForeground(myId, notificationBuilder.Build());

        // Listen for battery changes from the OS
        RegisterBatteryReceiver();

        // Periodic fallback: re-check battery even if broadcasts stop
        StartPeriodicCheck();

        _ = AppLogService.Instance.LogAsync("Background service started.");

        return StartCommandResult.Sticky;
    }


    private void AcquireWakeLock()
    {
        if (wakeLock == null)
        {
            var powerManager = (PowerManager?)GetSystemService(PowerService);
            wakeLock = powerManager?.NewWakeLock(WakeLockFlags.Partial, "BatteryMonitor::ServiceWakeLock");
            wakeLock?.Acquire();
        }
    }


    private void ReleaseWakeLock()
    {
        if (wakeLock is { IsHeld: true })
        {
            wakeLock.Release();
            wakeLock = null;
        }
    }


    private void StartPeriodicCheck()
    {
        handler ??= new Handler(Looper.MainLooper!);
        handler.RemoveCallbacksAndMessages(null);
        handler.PostDelayed(PeriodicCheckRunnable, Constants.TimerPeriod);
    }


    private void StopPeriodicCheck()
    {
        handler?.RemoveCallbacksAndMessages(null);
        handler = null;
    }


    private void PeriodicCheckRunnable()
    {
        CheckBattery();
        handler?.PostDelayed(PeriodicCheckRunnable, Constants.TimerPeriod);
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
        StopPeriodicCheck();
        UnregisterBatteryReceiver();
        ReleaseWakeLock();
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


    /// <summary>
    /// Action of the service: condition and what to do
    /// </summary>
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
}
