using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using BatteryMonitor.Platforms.Android;
using BatteryMonitor.Shared;
using BatteryMonitor.ViewModel;

namespace BatteryMonitor
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        //private BatteryMonitorViewModel viewModel;

        public MainActivity()
        {
            AndroidServiceManager.MainActivity = this;
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            ProcessIntent(intent);
        }


        private void ProcessIntent(Intent intent)
        {
            // Extract data from the intent and use it
            // For example, you can check for a specific action or extract extras
            if (intent != null)
            {
                // Example: checking for a specific action
                var action = intent.Action;
                if (action == "USER_TAPPED_NOTIFICATION")
                {
                    // Handle the specific action
                    //viewModel.BatteryPercentage = BatteryUtility.GetBatteryLevel();
                }
            }
        }


        public void StartService()
        {
            //this.viewModel = viewModel;
            var serviceIntent = new Intent(this, typeof(BackgroundService));
            serviceIntent.PutExtra("inputExtra", "Background Service");
            //serviceIntent.PutExtra("lowLevelValue", viewModel.MinLimit);
            //serviceIntent.PutExtra("highLevelValue", viewModel.MaxLimit);

#if ANDROID21_0_OR_GREATER
            StartForegroundService(serviceIntent);
#else
            StartService(serviceIntent);
#endif
            AndroidServiceManager.IsRunning = true;
        }


        public void StopService()
        {
            var serviceIntent = new Intent(this, typeof(BackgroundService));
            StopService(serviceIntent);

            AndroidServiceManager.IsRunning = false;
        }
    }
}
