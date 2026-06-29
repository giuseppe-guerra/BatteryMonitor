using Android.App;
using Android.OS;
using Android.Runtime;
using BatteryMonitor.Languages;

namespace BatteryMonitor
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public static readonly string ChannelIdService = "batteryBackgroundServiceChannel";
        public static readonly string ChannelIdAlerts = "batteryAlertChannel";

        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override void OnCreate()
        {
            base.OnCreate();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
#pragma warning disable CA1416
                var serviceChannel = new NotificationChannel(ChannelIdService, Strings.NotificationChannelName, NotificationImportance.Low);
                serviceChannel.SetShowBadge(false);

                var alertChannel = new NotificationChannel(ChannelIdAlerts, Strings.NotificationTitle, NotificationImportance.High);
                alertChannel.EnableVibration(true);
                alertChannel.LockscreenVisibility = NotificationVisibility.Public;

                if (GetSystemService(NotificationService) is NotificationManager manager)
                {
                    manager.CreateNotificationChannel(serviceChannel);
                    manager.CreateNotificationChannel(alertChannel);
                }
#pragma warning restore CA1416
            }
        }
    }
}
