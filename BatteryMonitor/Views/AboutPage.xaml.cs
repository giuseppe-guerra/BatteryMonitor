using System.Windows.Input;

namespace BatteryMonitor;

public partial class AboutPage : ContentPage
{
    public AboutPage()
	{
		InitializeComponent();
        lblVersion.Text = AppInfo.Current.VersionString;
    }

    private void GitHubTapGestureRecognizer(object sender, TappedEventArgs e)
    {
        Task.Run(() => Launcher.OpenAsync(Shared.Constants.GITHUB_URL));
    }

    protected override bool OnBackButtonPressed()
    {
        // Chiudere l'app invece di tornare indietro è veramente fastidioso.
        Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        return true;
    }

    private void EmailTapGestureRecognizer(object sender, TappedEventArgs e)
    {
        Task.Run(() => Launcher.OpenAsync($"mailto:{Shared.Constants.EMAIL}"));
    }
}