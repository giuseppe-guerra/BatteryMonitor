using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.Core.Content;
using BatteryMonitor.Languages;


namespace BatteryMonitor.Platforms.Android;

[BroadcastReceiver(Name= "com.toyokenstudio.batterymonitor.BootReceiverNS", Exported = true, Enabled = true, DirectBootAware = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted }, Priority = (int)IntentFilterPriority.HighPriority)]
public class BootReceiverNS : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent?.Action == Intent.ActionBootCompleted)
        {
            var serviceIntent = new Intent(context, typeof(BackgroundService));
            
            serviceIntent.AddFlags(ActivityFlags.NewTask);

            Toast.MakeText(context, $"{Strings.AppTitle} - {Strings.BootReceiverStartMessage}", ToastLength.Long)?.Show();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O) 
            { 
                ContextCompat.StartForegroundService(context, serviceIntent);
            }
            else
            {
                context.StartService(serviceIntent);
            }

        }
    }
}
