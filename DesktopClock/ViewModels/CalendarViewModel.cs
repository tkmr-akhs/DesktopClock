using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopClock.Core.Models;

namespace DesktopClock.ViewModels;

public partial class CalendarViewModel : ObservableRecipient
{
    private readonly ILoggingService _loggingService;
    private readonly ICalendarStyleSelectorService _calendarStyleSelectorService;
    private readonly IMonthlyCalendarService _monthlyCalendarService;

    public MonthlyCalendar MonthlyCalendar { get; }

    public ICommand NextMonthCommand { get; }

    public ICommand PreviousMonthCommand { get; }

    public ICommand BackToThisMonthCommand { get; }

    public ICommand ReloadScheduleCommand { get; }


    public CalendarViewModel(ILoggingService loggingService, ICalendarStyleSelectorService calendarStyleSelectorService, IMonthlyCalendarService monthlyCalendarService)
    {
        _loggingService = loggingService;
        _calendarStyleSelectorService = calendarStyleSelectorService;
        _monthlyCalendarService = monthlyCalendarService;

        _calendarStyleSelectorService.StyleChanged += _calendarStyleSelectorService_StyleChanged;

        var today = DateTime.Today;
        MonthlyCalendar = _monthlyCalendarService.MonthlyCalendar;
        _monthlyCalendarService.ApplyScheduleAsync();

        NextMonthCommand = new RelayCommand(
            async () =>
            {
                MonthlyCalendar.Next();
            });

        PreviousMonthCommand = new RelayCommand(
            async () =>
            {
                MonthlyCalendar.Previous();
            });

        BackToThisMonthCommand = new RelayCommand(
            async () =>
            {
                var today = DateTime.Today;
                MonthlyCalendar.JumpTo(today.Year, today.Month);
            });

        ReloadScheduleCommand = new RelayCommand(
            async () =>
            {
                await _monthlyCalendarService.ApplyScheduleAsync();
            });
    }

    private async void _calendarStyleSelectorService_StyleChanged(object? sender, EventArgs e)
    {
        await _loggingService.WriteLogAsync(nameof(CalendarViewModel), nameof(_calendarStyleSelectorService_StyleChanged), severity: Services.LogSeverity.Debug);
        await _monthlyCalendarService.ApplyScheduleAsync();
    }
}
