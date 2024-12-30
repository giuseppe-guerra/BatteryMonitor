using BatteryMonitor.Shared;

namespace BatteryMonitor
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            LoadThemePreference();
        }

        private void LoadThemePreference()
        {
            ThemeSwitch.Toggled -= OnThemeSwitchToggled;

            if (Preferences.ContainsKey(Constants.APP_THEME))
            {
                var savedPreference = Preferences.Get(Constants.APP_THEME, false);
                Application.Current.UserAppTheme = savedPreference ? AppTheme.Dark : AppTheme.Light;
                ThemeSwitch.IsToggled = savedPreference;
            }
            else
            {
                ThemeSwitch.IsToggled = Application.Current.RequestedTheme == AppTheme.Dark;
            }

            ThemeSwitch.Toggled += OnThemeSwitchToggled;
        }


        private void OnThemeSwitchToggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
            else
            {
                Application.Current.UserAppTheme = AppTheme.Light;
            }

            Preferences.Set(Constants.APP_THEME, e.Value);
        }
    }
}
