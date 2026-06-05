using Android.App;
using Android.Content;

namespace BatteryMonitor.Platforms.Android;

[BroadcastReceiver(Exported = false)]
public class BatteryCheckAlarmReceiver : BroadcastReceiver
{
    public const string ActionCheckBattery = "com.toyokenstudio.batterymonitor.CHECK_BATTERY";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent?.Action != ActionCheckBattery || context == null)
            return;

        var serviceIntent = new Intent(context, typeof(BackgroundService));
        serviceIntent.SetAction(ActionCheckBattery);
        context.StartService(serviceIntent);
    }
}
