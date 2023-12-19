using DesktopClock.Core.Models;
using DesktopClock.Core.Helpers;

namespace DesktopClock.Services;

public class MonthlyCalendarService : IMonthlyCalendarService
{
    private readonly ILoggingService _loggingService;
    private readonly IGoogleCalendarService _googleCalendarService;

    public event EventHandler<EventArgs> ScheduleApplied;

    public MonthlyCalendar MonthlyCalendar { get; }

    public HolidayDurationCalculator HolidayDurationCalculator { get; }

    private CancellationTokenSource googleCalendarCancellationTokenSource;

    public MonthlyCalendarService(ILoggingService loggingService, IGoogleCalendarService googleCalendarService)
    {
        _loggingService = loggingService;
        _googleCalendarService = googleCalendarService;

        MonthlyCalendar = new();
        MonthlyCalendar.PropertyChanged += MonthlyCalendar_PropertyChanged;

        HolidayDurationCalculator = new(MonthlyCalendar);
    }

    private async void MonthlyCalendar_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MonthlyCalendar.Month))
        {
            try
            {
                await _loggingService.WriteLogAsync(nameof(MonthlyCalendarService), nameof(MonthlyCalendar_PropertyChanged), $"{MonthlyCalendar.Year} {MonthlyCalendar.Month}");
                await ApplyScheduleAsync();
            }
            catch (Exception exp)
            {
                await _loggingService.WriteLogAsync(nameof(MonthlyCalendarService), nameof(MonthlyCalendar_PropertyChanged), exception: exp);
            }
        }
    }

    public async Task ApplyScheduleAsync()
    {
        if (googleCalendarCancellationTokenSource != null && !googleCalendarCancellationTokenSource.IsCancellationRequested) googleCalendarCancellationTokenSource.Cancel();
        googleCalendarCancellationTokenSource = new();
        await _googleCalendarService.ApplyScheduleToMonthlyCalendar(MonthlyCalendar,  googleCalendarCancellationTokenSource.Token);
        OnScheduleApplied();
    }

    private void OnScheduleApplied()
    {
        ScheduleApplied?.Invoke(this, EventArgs.Empty);
    }
}
