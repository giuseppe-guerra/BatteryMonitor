using BatteryMonitor.Shared;
using BatteryMonitor.ViewModel;

namespace BatteryMonitor;

public partial class SettingsPage : ContentPage
{
    private readonly BatteryMonitorViewModel viewModel;

    public SettingsPage(BatteryMonitorViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;

        this.viewModel = viewModel;
    }

    private async void btnSave_Clicked(object sender, EventArgs e)
    {
        SetPreferences();
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }

    private void SetPreferences()
    {
        Preferences.Default.Set(Constants.MIN_VALUE, viewModel.MinLimit);
        Preferences.Default.Set(Constants.MAX_VALUE, viewModel.MaxLimit);
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
        SetPreferences();
    }

    protected override bool OnBackButtonPressed()
    {
        // Chiudere l'app invece di tornare indietro è veramente fastidioso.
        Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        return true;
    }
}