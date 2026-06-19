using BatteryMonitor.Data;

namespace BatteryMonitor;

public partial class LogsPage : ContentPage
{
    private readonly AppLogService _logService;
    private IDispatcherTimer? _refreshTimer;

    public LogsPage(AppLogService logService)
    {
        InitializeComponent();
        _logService = logService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLogsAsync();
        StartAutoRefresh();
    }

    protected override void OnDisappearing()
    {
        StopAutoRefresh();
        base.OnDisappearing();
    }

    private void StartAutoRefresh()
    {
        _refreshTimer = Dispatcher.CreateTimer();
        _refreshTimer.Interval = TimeSpan.FromSeconds(10);
        _refreshTimer.Tick += async (s, e) => await LoadLogsAsync();
        _refreshTimer.Start();
    }

    private void StopAutoRefresh()
    {
        _refreshTimer?.Stop();
        _refreshTimer = null;
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
        bool confirm = await DisplayAlertAsync("Clear Logs", "Are you sure you want to delete all logs?", "Yes", "No");
        if (confirm)
        {
            await _logService.ClearLogsAsync();
            await LoadLogsAsync();
        }
    }
}
