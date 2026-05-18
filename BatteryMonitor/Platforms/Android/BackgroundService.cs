using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using BatteryMonitor.Languages;
using BatteryMonitor.Shared;

namespace BatteryMonitor.Platforms.Android;

// Explicit Java service name and exported setting so Android matches the
// manifest entry and delivers notifications properly (including to wearables).
[Service(Name = "com.toyokenstudio.batterymonitor.BackgroundService", Exported = false, ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeSpecialUse)]
internal class BackgroundService : Service
{
    Timer timer = null;
    int myId = (new object()).GetHashCode();
    private readonly IBinder binder;
    int lowLevel = 0;
    int highLevel = 0;

    public BackgroundService()
    {
        binder = new LocalBinder(this);
    }


    public class LocalBinder : Binder
    {
        readonly BackgroundService service;

        public BackgroundService GetService() => service;

        public LocalBinder(BackgroundService service) => this.service = service;
    }


    public override IBinder? OnBind(Intent? intent)
    {
        return binder;
    }


    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        // Ensure notification channel exists (in case Application.OnCreate didn't run or manifest wiring changed)
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
#pragma warning disable CA1416
            var serviceChannelForLevelChanges = new NotificationChannel(MainApplication.ChannelIdLevelChanges, Strings.NotificationChannelName, NotificationImportance.High);

            if (GetSystemService(NotificationService) is NotificationManager manager)
            {
                manager.CreateNotificationChannel(serviceChannelForLevelChanges);
            }
#pragma warning restore CA1416
        }

        var notificationIntent = new Intent(this, typeof(MainActivity));
        notificationIntent.SetAction("USER_TAPPED_NOTIFICATION");

        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        var notification = new NotificationCompat.Builder(this, MainApplication.ChannelIdLevelChanges)
            .SetSmallIcon(Resource.Drawable.iconbattery32)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetOngoing(true)
            .SetCategory(Notification.CategoryService)
            .SetContentTitle(Strings.AppTitle)
            .SetContentText(Strings.ServiceStarted)
            .SetContentIntent(pendingIntent);

        StartForeground(myId, notification.Build());

        // Mark service as running so app UI can reflect correct state
        AndroidServiceManager.IsRunning = true;
        // Persist service running state so app can know service was started (e.g. after reboot)
        Preferences.Default.Set(Constants.SERVICE_RUNNING, true);

        // Start timer. Do not pass the NotificationCompat.Builder instance between threads
        // because it's not guaranteed to be thread-safe. Build notifications on the main
        // thread when needed.
        timer = new Timer(Timer_Elapsed, null, 0, Constants.TimerPeriod);

        return StartCommandResult.Sticky;
    }


    void Timer_Elapsed(object state)
    {
        CheckBattery(state);
    }


    /// <summary>
    /// Action of the service: condition and what to do
    /// </summary>
    /// <param name="state"></param>
    private void CheckBattery(object state)
    {
#if DEBUG
        MainThread.BeginInvokeOnMainThread(() =>
            Toast.MakeText(Platform.AppContext, "Battery Monitor checking...", ToastLength.Short)?.Show()
        );
#endif

        var batteryLevel = BatteryUtility.GetBatteryLevel();

        lowLevel = Preferences.Default.Get(Constants.MIN_VALUE, DefaultSettings.LowLevelWarningValue);
        highLevel = Preferences.Default.Get(Constants.MAX_VALUE, DefaultSettings.HighLevelWarningValue);

        // Build and post notifications on the main thread to avoid thread-safety issues
        if (batteryLevel <= lowLevel && BatteryUtility.GetBatteryStatus() != BatteryState.Charging)
        {
            var text = $"{Strings.WarningLowLevel} ({batteryLevel}%)";
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var notificationIntent = new Intent(this, typeof(MainActivity));
                notificationIntent.SetAction("USER_TAPPED_NOTIFICATION");
                var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

                // Use a non-ongoing notification for threshold alerts so they can be mirrored
                // to connected devices (wearables) and behave like a normal alert.
                var builder = new NotificationCompat.Builder(this, MainApplication.ChannelIdLevelChanges)
                    .SetSmallIcon(Resource.Drawable.iconbattery32)
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetOngoing(false)
                    .SetCategory(Notification.CategoryRecommendation)
                    .SetAutoCancel(true)
                    .SetContentTitle(Strings.NotificationTitle)
                    .SetContentText(text)
                    .SetContentIntent(pendingIntent);

                StartForeground(myId, builder.Build());
            });
        }

        if (batteryLevel >= highLevel && BatteryUtility.GetBatteryStatus() == BatteryState.Charging)
        {
            var text = $"{Strings.WarningHighLevel} ({batteryLevel}%)";
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var notificationIntent = new Intent(this, typeof(MainActivity));
                notificationIntent.SetAction("USER_TAPPED_NOTIFICATION");
                var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

                var builder = new NotificationCompat.Builder(this, MainApplication.ChannelIdLevelChanges)
                    .SetSmallIcon(Resource.Drawable.iconbattery32)
                    .SetPriority(NotificationCompat.PriorityHigh)
                    .SetOngoing(false)
                    .SetCategory(Notification.CategoryRecommendation)
                    .SetAutoCancel(true)
                    .SetContentTitle(Strings.NotificationTitle)
                    .SetContentText(text)
                    .SetContentIntent(pendingIntent);

                StartForeground(myId, builder.Build());
            });
        }
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
