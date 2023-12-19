using System.Collections.ObjectModel;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using DesktopClock.Core.Models;
using DesktopClock.Models;

namespace DesktopClock.Services;

public class GoogleCalendarService : IGoogleCalendarService
{
    private const string ApplicationName = "DesktopClock for Windows";
    private const string SettingsKey = "GoogleCalendarDisplaySettings";

    public ObservableCollection<GoogleCalendarSetting> CalendarSettings => _calendarSettingsDictionary.ObservableValues;

    private readonly ILoggingService _loggingService;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IGooglePkceService _googlePkceService;
    private CalendarService _calendarService;
    private readonly ObservableDictionary<string, GoogleCalendarSetting> _calendarSettingsDictionary;

    public GoogleCalendarService(ILoggingService loggingService, ILocalSettingsService localSettingsService, IGooglePkceService googlePkceService)
    {
        _loggingService = loggingService;
        _localSettingsService = localSettingsService;
        _googlePkceService = googlePkceService;

        _googlePkceService.AuthenticationRequiredChanged += _googlePkceService_AuthenticationRequiredChanged;
        _calendarSettingsDictionary = new ObservableDictionary<string, GoogleCalendarSetting>();
    }

    private async void _googlePkceService_AuthenticationRequiredChanged(object? sender, GooglePkceService.AuthenticationRequiredChangedEventArgs e)
    {
        if (e.IsAuthenticationRequired)
        {
            await InitializeAsync();
        }
        else
        {
            _calendarSettingsDictionary.Clear();
            await SaveDictionaryToSettingsAsync();
        }
    }

    public async Task InitializeAsync()
    {
        if (_googlePkceService.IsAuthenticationRequired) {
            await LoadDictionaryFromSettingsAsync();
        }
    }

    private async Task InitializeGoogleCalendarServiceAsync()
    {
        if (_calendarService != default) return;

        var credential = await _googlePkceService.AuthenticateAsync(CancellationToken.None);

        if (credential != default)
        {
            _calendarService = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            await SyncGoogleCalendarsAsync(_calendarService);
        }
    }

    private async Task SyncGoogleCalendarsAsync(CalendarService calendarService)
    {
        var calendarList = await calendarService.CalendarList.List().ExecuteAsync();

        foreach (CalendarListEntry entry in calendarList.Items)
        {
            if (_calendarSettingsDictionary.ContainsKey(entry.Id))
            {
                _calendarSettingsDictionary[entry.Id].Name = entry.Summary;
            }
            else
            {
                _calendarSettingsDictionary[entry.Id] = new GoogleCalendarSetting(entry.Id, entry.Summary);
                _calendarSettingsDictionary[entry.Id].PropertyChanged += GoogleCalendarSetting_PropertyChanged;
            }
        }

        FlushOldGoogleCalendarSettings(calendarList);
        await SaveDictionaryToSettingsAsync();
    }

    private async void GoogleCalendarSetting_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            await SaveDictionaryToSettingsAsync();
        }
        catch (Exception exp)
        {
            await _loggingService.WriteLogAsync(nameof(GoogleCalendarService), nameof(GoogleCalendarSetting_PropertyChanged), exception: exp);
        }
    }

    public async Task ApplyScheduleToMonthlyCalendar(MonthlyCalendar monthlyCalendar, CancellationToken cancellationToken)
    {
        await InitializeGoogleCalendarServiceAsync();

        if (_calendarService == default) return;

        var complete = false;
        while (!complete && !cancellationToken.IsCancellationRequested)
        {
            monthlyCalendar.Clear();
            foreach (var keyValuePair in _calendarSettingsDictionary)
            {
                var displayType = keyValuePair.Value.DisplayType;

                if (displayType == GoogleCalendarDisplayType.Hidden) continue;

                var request = _calendarService.Events.List(keyValuePair.Key);
                request.TimeMin = monthlyCalendar.MinDate.ToDateTime(TimeOnly.MinValue);
                request.TimeMax = monthlyCalendar.MaxDate.AddDays(-1).ToDateTime(TimeOnly.MaxValue);

                var events = await request.ExecuteAsync();

                try
                {
                    foreach (var eventItem in events.Items)
                    {
                        var eventRange = ToDateOnlyRange(eventItem.Start, eventItem.End, monthlyCalendar.MinDate, monthlyCalendar.MaxDate.AddDays(1));
                        if (displayType == GoogleCalendarDisplayType.Events)
                        {
                            foreach (var day in eventRange.GetAllDatesInRange())
                            {
                                monthlyCalendar.MarkEntry(day, isScheduledDay: true);
                            }
                        }
                        else if (displayType == GoogleCalendarDisplayType.NonWorkingDay)
                        {
                            foreach (var day in eventRange.GetAllDatesInRange())
                            {
                                monthlyCalendar.MarkEntry(day, isNonWorkingDay: true);
                                monthlyCalendar.AddEntryInformation(day, eventItem.Summary);
                            }
                        }
                    }
                    complete = true;
                }
                catch (ArgumentException exp)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    else
                    {
                        await _loggingService.WriteLogAsync(nameof(GoogleCalendarService), nameof(ApplyScheduleToMonthlyCalendar), exception: exp);
                        throw;
                    }
                }
            }

        }
    }

    private DateOnlyRange ToDateOnlyRange(EventDateTime start, EventDateTime finish, DateOnly windowStart, DateOnly windowFinish)
    {
        DateOnly startDateOnly;
        DateOnly finishDateOnly;

        if (!String.IsNullOrEmpty(start.Date))
        {
            // 終日イベント
            startDateOnly = DateOnly.Parse(start.Date);
            finishDateOnly = DateOnly.Parse(finish.Date);
        }
        else
        {
            // 時間指定イベント
            startDateOnly = DateOnly.FromDateTime(DateTime.Parse(start.DateTimeRaw));

            var finishDateTime = DateTime.Parse(start.DateTimeRaw);
            var finishTimeOnly = TimeOnly.FromDateTime(finishDateTime);
            finishDateOnly = DateOnly.FromDateTime(finishDateTime);

            if (finishTimeOnly != TimeOnly.MinValue) finishDateOnly = finishDateOnly.AddDays(1);
        }

        return new DateOnlyRange(Max(startDateOnly, windowStart), Min(finishDateOnly, windowFinish));
    }

    private DateOnly Min(DateOnly a, DateOnly b)
    {
        return a < b ? a : b;
    }

    private DateOnly Max(DateOnly a, DateOnly b)
    {
        return a < b ? b : a;
    }

    private void FlushOldGoogleCalendarSettings(CalendarList calendarList)
    {
        foreach (var setting in _calendarSettingsDictionary)
        {
            var found = false;
            foreach (var calendar in calendarList.Items)
            {
                if (calendar.Id == setting.Key)
                {
                    found = true;
                    break;
                }
            }

            if (!found) _calendarSettingsDictionary.Remove(setting.Key);
        }
    }

    private async Task LoadDictionaryFromSettingsAsync()
    {
        var calendarSettingsDictionaryOrNull = await _localSettingsService.ReadSettingAsync<ObservableDictionary<string, GoogleCalendarSetting>>(SettingsKey);
        var calendarSettingsDictionary = calendarSettingsDictionaryOrNull ?? default;

        if (calendarSettingsDictionary == default) return;

        _calendarSettingsDictionary.Clear();

        foreach (var keyValuePair in calendarSettingsDictionary)
        {
            _calendarSettingsDictionary[keyValuePair.Key] = keyValuePair.Value;
            _calendarSettingsDictionary[keyValuePair.Key].PropertyChanged += GoogleCalendarSetting_PropertyChanged;
        }
    }

    private async Task SaveDictionaryToSettingsAsync()
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, _calendarSettingsDictionary);
    }

}
