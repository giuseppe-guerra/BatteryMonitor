using BatteryMonitor.Data;
using BatteryMonitor.Languages;
using BatteryMonitor.Shared;

namespace BatteryMonitor;

public partial class LogsPage : ContentPage
{
    private readonly AppLogService _logService;
    private IDispatcherTimer? _refreshTimer;

    public LogsPage(AppLogService logService)
    {
        InitializeComponent();
        _logService = logService;
        LogSwitch.IsToggled = Preferences.Default.Get(Constants.LOG_ENABLED, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LogSwitch.IsToggled = Preferences.Default.Get(Constants.LOG_ENABLED, false);
        await LoadLogsAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private void LogSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        Preferences.Default.Set(Constants.LOG_ENABLED, e.Value);
    }

    private async Task LoadLogsAsync()
    {
        var logs = await _logService.GetLogsAsync();
        LogsCollection.ItemsSource = logs;
    }

    private async void LogsRefreshView_Refreshing(object sender, EventArgs e)
    {
        await LoadLogsAsync();
        LogsRefreshView.IsRefreshing = false;
    }

    private async void btnRefreshLogs_Clicked(object sender, EventArgs e)
    {
        await LoadLogsAsync();
    }

    private async void btnClearLogs_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(Strings.ClearLogs, Strings.ClearLogsConfirmation, Strings.Yes, Strings.No);
        if (confirm)
        {
            await _logService.ClearLogsAsync();
            await LoadLogsAsync();
        }
    }
}
