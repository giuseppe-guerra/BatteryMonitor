using Android.App;
using Android.OS;
using Android.Runtime;
using BatteryMonitor.Languages;

namespace BatteryMonitor
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public static readonly string ChannelIdLevelChanges = "batteryBackgroundServiceChannel";
        // public static readonly string ChannelIdServiceDown = "batteryBackgroundServiceDown";

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
                var serviceChannelForLevelChanges = new NotificationChannel(ChannelIdLevelChanges, Strings.NotificationChannelName, NotificationImportance.High);

                if (GetSystemService(NotificationService) is NotificationManager manager)
                {
                    manager.CreateNotificationChannel(serviceChannelForLevelChanges);
                }
#pragma warning restore CA1416
            }
        }
    }
}
