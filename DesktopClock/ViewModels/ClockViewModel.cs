using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopClock.Core.Contracts.Services;
using DesktopClock.Core.Models;
using DesktopClock.Models;

namespace DesktopClock.ViewModels;

public partial class ClockViewModel : ObservableRecipient
{
    private readonly ILoggingService _loggingService;
    private readonly IScreenChangeDetectionService _screenChangeDetectionService;
    private readonly IHourStyleSelectorService _hourStyleSelectorService;
    private readonly IMinuteStyleSelectorService _minuteStyleSelectorService;
    private readonly IDateStyleSelectorService _dateStyleSelectorService;
    private readonly ICalendarStyleSelectorService _calendarStyleSelectorService;
    private readonly IDateTimeProviderService _dateTimeProviderService;
    private readonly IMonthlyCalendarService _monthlyCalendarService;

    [ObservableProperty]
    private char _hourTens;

    [ObservableProperty]
    private char _hourOnes;

    [ObservableProperty]
    private char _minuteTens;

    [ObservableProperty]
    private char _minuteOnes;

    [ObservableProperty]
    private CalendarEntry _todayCalendarEntry;

    [ObservableProperty]
    private string _todayHolidayInformation;

    [ObservableProperty]
    private string _todayScheduleInformation;

    [ObservableProperty]
    private string _futureHolidayInformation;

    [ObservableProperty]
    private string _futureScheduleInformation;

    private readonly string _nDayHolidayFormat;
    private readonly string _futureHolidayFormat;
    private readonly string _plansFormat;
    private readonly string _planned;
    private readonly string _plans;
    private readonly string _nameToday;
    private readonly string _nameTomorrow;
    private readonly string _nameTheDayAfterTomorrow;

    public ClockViewModel(ILoggingService loggingService, IScreenChangeDetectionService screenChangeDetectionService, IHourStyleSelectorService hourStyleSelectorService,IMinuteStyleSelectorService minuteStyleSelectorService, IDateStyleSelectorService dateStyleSelectorService, ICalendarStyleSelectorService calendarStyleSelectorService, IDateTimeProviderService dateTimeProviderService, IMonthlyCalendarService monthlyCalendarService)
    {
        _loggingService = loggingService;
        _screenChangeDetectionService = screenChangeDetectionService;
        _hourStyleSelectorService = hourStyleSelectorService;
        _minuteStyleSelectorService = minuteStyleSelectorService;
        _dateStyleSelectorService = dateStyleSelectorService;
        _calendarStyleSelectorService = calendarStyleSelectorService;
        _dateTimeProviderService = dateTimeProviderService;
        _monthlyCalendarService = monthlyCalendarService;

        _screenChangeDetectionService.ScreenChanged += _screenChangeDetectionService_ScreenChanged;
        _hourStyleSelectorService.StyleChanged += _hourStyleSelectorService_StyleChanged;
        _minuteStyleSelectorService.StyleChanged += _minuteStyleSelectorService_StyleChanged;
        _dateStyleSelectorService.StyleChanged += _dateStyleSelectorService_StyleChanged;
        _calendarStyleSelectorService.StyleChanged += _calendarStyleSelectorService_StyleChanged;
        _dateTimeProviderService.PropertyChanged += _dateTimeProviderService_PropertyChanged;
        _monthlyCalendarService.ScheduleApplied += _monthlyCalendarService_ScheduleApplied;

        var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
        _nDayHolidayFormat = resourceLoader.GetString("ClockPage_NDayHolidayFormat");
        _futureHolidayFormat = resourceLoader.GetString("ClockPage_FutureHolidayFormat");
        _plansFormat = resourceLoader.GetString("ClockPage_PlansFormat");
        _nameToday = resourceLoader.GetString("Today");
        _nameTomorrow = resourceLoader.GetString("Tomorrow");
        _nameTheDayAfterTomorrow = resourceLoader.GetString("TheDayAfterTomorrow");
        _planned = resourceLoader.GetString("Planned");
        _plans = resourceLoader.GetString("Plans");

        SetTimeProperties();
        TodayCalendarEntry = _monthlyCalendarService.MonthlyCalendar.GetEntry(DateOnly.FromDateTime(_dateTimeProviderService.Today));
        TodayHolidayInformation = String.Empty;
        TodayScheduleInformation = String.Empty;
        FutureHolidayInformation = String.Empty;
        FutureScheduleInformation = String.Empty;
    }

    #region Event Handlers

    private async void _dateTimeProviderService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is IDateTimeProviderService dateTime)
        {
            if (e.PropertyName == nameof(IDateTimeProviderService.Minute))
            {
                await _loggingService.WriteLogAsync(nameof(ClockViewModel), nameof(_dateTimeProviderService_PropertyChanged), $"Minute changed. {MinuteTens}{MinuteOnes} => {dateTime.Minute}", severity: Services.LogSeverity.Verbose);
                var minute = dateTime.Minute;
                MinuteTens = GetTens(minute);
                MinuteOnes = GetOnes(minute);
            }
            else if (e.PropertyName == nameof(IDateTimeProviderService.Hour))
            {
                await _loggingService.WriteLogAsync(nameof(ClockViewModel), nameof(_dateTimeProviderService_PropertyChanged), $"Hour changed. {HourTens}{HourOnes} => {dateTime.Hour}", severity: Services.LogSeverity.Debug);
                var hour = dateTime.Hour;
                HourTens = GetTens(hour);
                HourOnes = GetOnes(hour);

                var today = dateTime.Today;
                _monthlyCalendarService.MonthlyCalendar.JumpTo(today.Year, today.Month, true);
            }
            else if (e.PropertyName == nameof(IDateTimeProviderService.Day))
            {
                await _loggingService.WriteLogAsync(nameof(ClockViewModel), nameof(_dateTimeProviderService_PropertyChanged), $"Day changed. {TodayCalendarEntry.Date} => {dateTime.Today}", severity: Services.LogSeverity.Debug);
                var today = dateTime.Today;

                await _loggingService.WriteLogAsync(nameof(ClockViewModel), nameof(_dateTimeProviderService_PropertyChanged), $"Invoke MonthlyCalendar.JumpTo(today.Year, today.Month, true)", severity: Services.LogSeverity.Debug);
                _monthlyCalendarService.MonthlyCalendar.JumpTo(today.Year, today.Month, true);
                // TodayCalendarEntry の更新は、
                // 1. MonthlyCalendar.JumpTo() で PropertyChanged イベントがトリガー
                // 2. MonthlyCalendarService 内で MonthlyCalendar.PropertyChanged イベントを購読しており、そのハンドラ内で ScheduleApplied イベントがトリガー
                // 3. this._monthlyCalendarService_ScheduleApplied で this.SetDateInformation() が実行される。

                await _loggingService.WriteLogAsync(nameof(ClockViewModel), nameof(_dateTimeProviderService_PropertyChanged), $"Invoke LoggingService.RemoveExpiredLogsAsync()", severity: Services.LogSeverity.Debug);
                await _loggingService.RemoveExpiredLogsAsync();
            }
        }
    }

    private async void _screenChangeDetectionService_ScreenChanged(object? sender, ScreenChangedEventArgs e)
    {
        await _loggingService.WriteLogAsync(nameof(ClockViewModel), nameof(_screenChangeDetectionService_ScreenChanged), severity: Services.LogSeverity.Debug);
        if (e.ChangedSize.HasFlag(ScreenChangedSize.Height))
        {
            await _hourStyleSelectorService.SetRequestedTextStyleAsync();
            await _minuteStyleSelectorService.SetRequestedTextStyleAsync();
            await _dateStyleSelectorService.SetRequestedTextStyleAsync();

            RefreshHour();
            RefreshMinute();
            RefreshDate();
            RefreshDateInformation();
        }
    }

    private　void _monthlyCalendarService_ScheduleApplied(object? sender, EventArgs e)
    {
        _loggingService.WriteLog(nameof(ClockViewModel), nameof(_monthlyCalendarService_ScheduleApplied), severity: Services.LogSeverity.Debug);

        if (_dateTimeProviderService.Today.Year == _monthlyCalendarService.MonthlyCalendar.Year && _dateTimeProviderService.Today.Month == _monthlyCalendarService.MonthlyCalendar.Month)
        {
            SetDateInformation();
        }
    }

    private void _hourStyleSelectorService_StyleChanged(object? sender, EventArgs e)
    {
        RefreshHour();
    }

    private void _minuteStyleSelectorService_StyleChanged(object? sender, EventArgs e)
    {
        RefreshMinute();
    }

    private void _dateStyleSelectorService_StyleChanged(object? sender, EventArgs e)
    {
        RefreshDate();
        RefreshDateInformation();
    }

    private void _calendarStyleSelectorService_StyleChanged(object? sender, EventArgs e)
    {
        RefreshDate();
        RefreshDateInformation();
    }

    #endregion

    #region Setter Methods

    private void SetTimeProperties()
    {
        var now = _dateTimeProviderService.Now;
        HourTens = GetTens(now.Hour);
        HourOnes = GetOnes(now.Hour);
        MinuteTens = GetTens(now.Minute);
        MinuteOnes = GetOnes(now.Minute);
    }

    private void RefreshHour()
    {
        var now = DateTime.Now;
        HourTens = GetDifferentNumber(HourTens);
        HourTens = GetTens(now.Hour);
        HourOnes = GetDifferentNumber(HourOnes);
        HourOnes = GetOnes(now.Hour);
    }

    private void RefreshMinute()
    {
        var now = DateTime.Now;
        MinuteTens = GetDifferentNumber(MinuteTens);
        MinuteTens = GetTens(now.Minute);
        MinuteOnes = GetDifferentNumber(MinuteOnes);
        MinuteOnes = GetOnes(now.Minute);
    }

    private void RefreshDate()
    {
        var tmp = TodayCalendarEntry;
        TodayCalendarEntry = CalendarEntry.Empty;
        TodayCalendarEntry = tmp;
    }

    private void RefreshDateInformation()
    {
        var tmp = TodayHolidayInformation;
        TodayHolidayInformation = String.Empty;
        TodayHolidayInformation = tmp;

        tmp = TodayScheduleInformation;
        TodayScheduleInformation = String.Empty;
        TodayScheduleInformation = tmp;

        tmp = FutureHolidayInformation;
        FutureHolidayInformation = String.Empty;
        FutureHolidayInformation = tmp;

        tmp = FutureScheduleInformation;
        FutureScheduleInformation = String.Empty;
        FutureScheduleInformation = tmp;
    }

    private void SetTodayHolidayInformation(string holidayName)
    {
        if (TodayHolidayInformation != holidayName) TodayHolidayInformation = holidayName;
    }

    private void SetTodayScheduleInformation(string plannedDayName)
    {
        if (TodayScheduleInformation != plannedDayName) TodayScheduleInformation = plannedDayName;
    }

    private void SetFutureHolidayInformation(string holidayName, string targetDay)
    {
        var message = String.IsNullOrEmpty(holidayName) ? String.Empty : String.Format(_futureHolidayFormat, holidayName, targetDay);
        if (FutureHolidayInformation != message) FutureHolidayInformation = message;
    }

    private void SetFutureHolidayInformation(int days, string startDay)
    {
        var message = days <= 0 ? String.Empty : String.Format(_nDayHolidayFormat, days, startDay);
        if (FutureHolidayInformation != message) FutureHolidayInformation = message;
    }

    private void SetFutureScheduleInformation(string plan, string targetDay)
    {
        var message = String.IsNullOrEmpty(plan) ? String.Empty : String.Format(_plansFormat, plan, targetDay);
        if (FutureScheduleInformation != message) FutureScheduleInformation = message;
    }

    private void SetDateInformation()
    {
        var today = DateOnly.FromDateTime(_dateTimeProviderService.Today);
        var Tomorrow = today.AddDays(1);

        TodayCalendarEntry = _monthlyCalendarService.MonthlyCalendar[today];
        var TomorrowCalEntry = _monthlyCalendarService.MonthlyCalendar[Tomorrow];

        if (TodayCalendarEntry.IsNonWorkingDay) SetTodayHolidayInformation(TodayCalendarEntry.Information);
        else SetTodayHolidayInformation(String.Empty);

        if (TodayCalendarEntry.IsScheduledDay) SetTodayScheduleInformation(_planned);
        else SetTodayScheduleInformation(String.Empty);

        if (TomorrowCalEntry.IsScheduledDay) SetFutureScheduleInformation(_plans, _nameTomorrow);
        else SetFutureScheduleInformation(String.Empty, String.Empty);

        var holidayDurationInfo = _monthlyCalendarService.HolidayDurationCalculator.CalculateHolidayDuration(TodayCalendarEntry.Date);

        if (holidayDurationInfo.TotalDays <= 0 || holidayDurationInfo.StartInDays > 2) SetFutureHolidayInformation(String.Empty, String.Empty);

        string targetDay = String.Empty;
        if (holidayDurationInfo.StartInDays <= 0) targetDay = _nameToday;
        else if (holidayDurationInfo.StartInDays == 1) targetDay = _nameTomorrow;
        else if (holidayDurationInfo.StartInDays == 2) targetDay = _nameTheDayAfterTomorrow;

        if (holidayDurationInfo.TotalDays == 1 && holidayDurationInfo.StartInDays > 0 && holidayDurationInfo.StartInDays < 3)
        {
            var targetDate = today.AddDays(holidayDurationInfo.StartInDays);
            var calEntry = _monthlyCalendarService.MonthlyCalendar[targetDate];

            var holidayName = calEntry.Information;

            SetFutureHolidayInformation(holidayName, targetDay);
        }
        else if (holidayDurationInfo.TotalDays == 1)
        {
            return;
        }
        else
        {
            var totalDays = holidayDurationInfo.StartInDays < 0 ? holidayDurationInfo.TotalDays + holidayDurationInfo.StartInDays : holidayDurationInfo.TotalDays;
            SetFutureHolidayInformation(totalDays, targetDay);
        }
    }

    #endregion

    #region Utillity Methods
    private char GetTens(int number)
    {
        return (number / 10).ToString()[0];
    }

    private char GetOnes(int number)
    {
        return (number % 10).ToString()[0];
    }

    private static char GetDifferentNumber(char numberChar)
    {
        var number = Int32.Parse(numberChar.ToString());
        return number == 0 ? 9.ToString()[0] : (number - 1).ToString()[0];
    }

    #endregion
}
