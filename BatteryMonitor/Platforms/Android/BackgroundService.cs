using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using BatteryMonitor.Languages;
using BatteryMonitor.Shared;

namespace BatteryMonitor.Platforms.Android;

//[Service]
[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeSpecialUse)]
internal class BackgroundService : Service
{
    Timer timer = null;
    int myId = (new object()).GetHashCode();
    private readonly IBinder binder = new LocalBinder();
    int lowLevel = 0;
    int highLevel = 0;


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
        //var input = intent.GetStringExtra("inputExtra");

        //lowLevel = intent.GetIntExtra("lowLevelValue", 0);
        //highLevel = intent.GetIntExtra("highLevelValue", 0);
        
        var notificationIntent = new Intent(this, typeof(MainActivity));
        notificationIntent.SetAction("USER_TAPPED_NOTIFICATION");

        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

        var notification = new NotificationCompat.Builder(this, MainApplication.ChannelIdLevelChanges)
            .SetSmallIcon(Resource.Drawable.iconbattery32)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetContentTitle(Strings.AppTitle)
            .SetContentText(Strings.ServiceStarted)
            .SetContentIntent(pendingIntent);

        StartForeground(myId, notification.Build());

        timer = new Timer(Timer_Elapsed, notification, 0, Constants.TimerPeriod);

        return StartCommandResult.Sticky;
    }


    void Timer_Elapsed(object state)
    {
        //AndroidServiceManager.IsRunning = true;

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

        if (batteryLevel <= lowLevel && BatteryUtility.GetBatteryStatus() != BatteryState.Charging)
        {
            var notification = (NotificationCompat.Builder)state;
            notification.SetContentTitle(Strings.NotificationTitle);
            notification.SetContentText($"{Strings.WarningLowLevel} ({batteryLevel}%)");
            StartForeground(myId, notification.Build());
        }

        if (batteryLevel >= highLevel && BatteryUtility.GetBatteryStatus() == BatteryState.Charging)
        {
            var notification = (NotificationCompat.Builder)state;
            notification.SetContentTitle(Strings.NotificationTitle);
            notification.SetContentText($"{Strings.WarningHighLevel} ({batteryLevel}%)");
            StartForeground(myId, notification.Build());
        }
    }
}
