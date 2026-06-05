using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using BatteryMonitor.Languages;
using BatteryMonitor.Shared;

namespace BatteryMonitor.Platforms.Android;

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeSpecialUse)]
internal class BackgroundService : Service
{
    int myId = (new object()).GetHashCode();
    private readonly IBinder binder = new LocalBinder();
    int lowLevel = 0;
    int highLevel = 0;
    DateTime lastNotificationTime = DateTime.MinValue;
    NotificationCompat.Builder? notificationBuilder;

    private const long CheckIntervalMs = (long)(1 * 60 * 1000); // 1 minute


    public class LocalBinder : Binder
    {
        public BackgroundService GetService() => this.GetService();
    }


    public override IBinder? OnBind(Intent? intent)
    {
        return binder;
    }


    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        if (intent?.Action == BatteryCheckAlarmReceiver.ActionCheckBattery)
        {
            // Alarm fired — perform the battery check, then schedule the next one
            CheckBattery();
            ScheduleNextAlarm();
            return StartCommandResult.Sticky;
        }

        // Initial start — build the foreground notification
        var notificationIntent = new Intent(this, typeof(MainActivity));
        notificationIntent.SetAction("USER_TAPPED_NOTIFICATION");

        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

        notificationBuilder = new NotificationCompat.Builder(this, MainApplication.ChannelIdLevelChanges)
            .SetSmallIcon(Resource.Drawable.iconbattery32)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetContentTitle(Strings.AppTitle)
            .SetContentText(Strings.ServiceStarted)
            .SetContentIntent(pendingIntent);

        StartForeground(myId, notificationBuilder.Build());

        // Schedule the first alarm
        ScheduleNextAlarm();

        return StartCommandResult.Sticky;
    }


    private void ScheduleNextAlarm()
    {
        var alarmManager = (AlarmManager?)GetSystemService(AlarmService);
        if (alarmManager == null) return;

        var intent = new Intent(this, typeof(BatteryCheckAlarmReceiver));
        intent.SetAction(BatteryCheckAlarmReceiver.ActionCheckBattery);

        var pendingIntent = PendingIntent.GetBroadcast(
            this, 0, intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var triggerAtMs = SystemClock.ElapsedRealtime() + CheckIntervalMs;

        alarmManager.SetExactAndAllowWhileIdle(
            AlarmType.ElapsedRealtimeWakeup,
            triggerAtMs,
            pendingIntent);
    }


    private void CancelAlarm()
    {
        var alarmManager = (AlarmManager?)GetSystemService(AlarmService);
        if (alarmManager == null) return;

        var intent = new Intent(this, typeof(BatteryCheckAlarmReceiver));
        intent.SetAction(BatteryCheckAlarmReceiver.ActionCheckBattery);

        var pendingIntent = PendingIntent.GetBroadcast(
            this, 0, intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        if (pendingIntent != null)
            alarmManager.Cancel(pendingIntent);
    }


    public override void OnDestroy()
    {
        CancelAlarm();
        base.OnDestroy();
    }


    /// <summary>
    /// Action of the service: condition and what to do
    /// </summary>
    private void CheckBattery()
    {
#if DEBUG
        MainThread.BeginInvokeOnMainThread(() =>
            Toast.MakeText(Platform.AppContext, "Battery Monitor checking...", ToastLength.Short)?.Show()
        );
#endif

        var batteryLevel = BatteryUtility.GetBatteryLevel();

        lowLevel = Preferences.Default.Get(Constants.MIN_VALUE, DefaultSettings.LowLevelWarningValue);
        highLevel = Preferences.Default.Get(Constants.MAX_VALUE, DefaultSettings.HighLevelWarningValue);

        var cooldownMinutes = Preferences.Default.Get(Constants.NOTIFICATION_COOLDOWN, DefaultSettings.NotificationCooldownMinutes);
        var now = DateTime.UtcNow;
        var elapsedTime = now - lastNotificationTime;
        var cooldownElapsed = elapsedTime.TotalMinutes >= cooldownMinutes;

        // Debug info
        if (!cooldownElapsed) System.Diagnostics.Debug.WriteLine($"Notification checked but delayed (elapsed ms: {elapsedTime.TotalMilliseconds}).");
        else System.Diagnostics.Debug.WriteLine($"Notification checked and showed (elapsed ms: {elapsedTime.TotalMilliseconds}).");
        // Debug info

        if (notificationBuilder == null) return;

        if (batteryLevel <= lowLevel && BatteryUtility.GetBatteryStatus() != BatteryState.Charging && cooldownElapsed)
        {
            notificationBuilder.SetContentTitle(Strings.NotificationTitle);
            notificationBuilder.SetContentText($"{Strings.WarningLowLevel} ({batteryLevel}%)");
            StartForeground(myId, notificationBuilder.Build());
            lastNotificationTime = now;
        }

        if (batteryLevel >= highLevel && BatteryUtility.GetBatteryStatus() == BatteryState.Charging && cooldownElapsed)
        {
            notificationBuilder.SetContentTitle(Strings.NotificationTitle);
            notificationBuilder.SetContentText($"{Strings.WarningHighLevel} ({batteryLevel}%)");
            StartForeground(myId, notificationBuilder.Build());
            lastNotificationTime = now;
        }
    }
}
